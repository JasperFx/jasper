using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Persistence;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Util;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenRetries : IDisposable, IRetries
    {
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _tables;
        private readonly ITransportLogger _logger;
        private readonly MessagingSettings _settings;
        private readonly ActionBlock<Envelope[]> _deleteIncoming;
        private readonly BatchingBlock<Envelope> _deleteIncomingBatching;
        private readonly ActionBlock<Envelope[]> _deleteOutgoing;
        private readonly BatchingBlock<Envelope> _deleteOutgoingBatching;
        private readonly ActionBlock<ErrorReport[]> _logErrorReport;
        private readonly BatchingBlock<ErrorReport> _logErrorReportBatching;
        private readonly ActionBlock<Envelope[]> _scheduleIncoming;
        private readonly BatchingBlock<Envelope> _scheduleIncomingBatching;

        // Strictly for testing
        public readonly ManualResetEvent IncomingDeleted = new ManualResetEvent(false);
        public readonly ManualResetEvent OutgoingDeleted = new ManualResetEvent(false);
        public readonly ManualResetEvent Scheduled = new ManualResetEvent(false);
        public readonly ManualResetEvent ErrorReportLogged = new ManualResetEvent(false);

        public MartenRetries(IDocumentStore store, EnvelopeTables tables, ITransportLogger logger, MessagingSettings settings)
        {
            _store = store;
            _tables = tables;
            _logger = logger;
            _settings = settings;

            _deleteIncoming = new ActionBlock<Envelope[]>(deleteIncoming);
            _deleteIncomingBatching = new BatchingBlock<Envelope>(250.Milliseconds(), _deleteIncoming, settings.Cancellation);

            _deleteOutgoing = new ActionBlock<Envelope[]>(deleteOutgoing);
            _deleteOutgoingBatching = new BatchingBlock<Envelope>(250.Milliseconds(), _deleteOutgoing, settings.Cancellation);

            _logErrorReport = new ActionBlock<ErrorReport[]>(logErrorReports);
            _logErrorReportBatching = new BatchingBlock<ErrorReport>(250.Milliseconds(), _logErrorReport, settings.Cancellation);

            _scheduleIncoming = new ActionBlock<Envelope[]>(scheduleIncoming);
            _scheduleIncomingBatching = new BatchingBlock<Envelope>(250.Milliseconds(), _scheduleIncoming, settings.Cancellation);
        }

        private async Task scheduleIncoming(Envelope[] envelopes)
        {
            try
            {
                using (var session = _store.LightweightSession())
                {
                    foreach (var envelope in envelopes)
                    {
                        session.ScheduleExecution(_tables.Incoming, envelope);
                    }

                    await session.SaveChangesAsync();

                    Scheduled.Set();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                foreach (var envelope in envelopes)
                {
                    ScheduleExecution(envelope);
                }
            }
        }

        private async Task deleteIncoming(Envelope[] envelopes)
        {
            try
            {
                using (var session = _store.LightweightSession())
                {
                    session.DeleteEnvelopes(_tables.Incoming, envelopes);
                    await session.SaveChangesAsync();

                    IncomingDeleted.Set();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                foreach (var envelope in envelopes)
                {
                    DeleteIncoming(envelope);
                }
            }
        }

        private async Task deleteOutgoing(Envelope[] envelopes)
        {
            try
            {
                using (var session = _store.LightweightSession())
                {
                    session.DeleteEnvelopes(_tables.Outgoing, envelopes);
                    await session.SaveChangesAsync();

                    OutgoingDeleted.Set();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                foreach (var envelope in envelopes)
                {
                    DeleteOutgoing(envelope);
                }
            }
        }

        private async Task logErrorReports(ErrorReport[] errors)
        {
            if (!_settings.PersistDeadLetterEnvelopes) return;

            try
            {
                using (var session = _store.LightweightSession())
                {
                    session.Store(errors);
                    session.DeleteEnvelopes(_tables.Incoming, errors.Select(x => x.Id).ToArray());
                    await session.SaveChangesAsync();

                    ErrorReportLogged.Set();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                foreach (var error in errors)
                {
                    LogErrorReport(error);
                }


            }
        }

        public void DeleteIncoming(Envelope envelope)
        {
            _deleteIncomingBatching.Post(envelope);
        }

        public void DeleteOutgoing(Envelope envelope)
        {
            _deleteOutgoingBatching.Post(envelope);
        }

        public void LogErrorReport(ErrorReport report)
        {
            if (!_settings.PersistDeadLetterEnvelopes) return;
            _logErrorReportBatching.Post(report);
        }

        public void Dispose()
        {
            _deleteIncoming.Complete();
            _deleteIncomingBatching.Complete();
            _deleteOutgoing.Complete();
            _deleteOutgoingBatching.Complete();
            _logErrorReport.Complete();
            _logErrorReportBatching.Complete();
            _scheduleIncoming.Complete();
            _scheduleIncomingBatching.Complete();

            _deleteIncomingBatching.Dispose();
            _deleteOutgoingBatching.Dispose();
            _logErrorReportBatching.Dispose();
            _scheduleIncomingBatching.Dispose();
        }

        public void ScheduleExecution(Envelope envelope)
        {
            _scheduleIncomingBatching.Post(envelope);
        }
    }
}
