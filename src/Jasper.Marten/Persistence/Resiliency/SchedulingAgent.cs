using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Marten;
using Marten.Services;
using Marten.Util;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class SchedulingAgent : IHostedService, IDisposable, ISchedulingAgent
    {
        private readonly IChannelGraph _channels;
        private readonly IWorkerQueue _workers;
        private readonly IDocumentStore _store;
        private readonly BusSettings _settings;
        private readonly CompositeTransportLogger _logger;
        private readonly StoreOptions _storeOptions;
        private readonly ActionBlock<IMessagingAction> _worker;
        private NpgsqlConnection _connection;
        private readonly RunScheduledJobs _scheduledJobs;
        private readonly RecoverIncomingMessages _incomingMessages;
        private readonly RecoverOutgoingMessages _outgoingMessages;
        private Timer _scheduledJobTimer;
        private Timer _nodeReassignmentTimer;
        private readonly ReassignFromDormantNodes _nodeReassignment;

        public SchedulingAgent(IChannelGraph channels, IWorkerQueue workers, IDocumentStore store, BusSettings settings, CompositeTransportLogger logger, StoreOptions storeOptions)
        {
            _channels = channels;
            _workers = workers;
            _store = store;
            _settings = settings;
            _logger = logger;
            _storeOptions = storeOptions;

            _worker = new ActionBlock<IMessagingAction>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });

            var marker = new OwnershipMarker(_settings, _storeOptions);
            _scheduledJobs = new RunScheduledJobs(_workers, _store, marker, logger);
            _incomingMessages = new RecoverIncomingMessages(_workers, _settings, marker, this, _logger);
            _outgoingMessages = new RecoverOutgoingMessages(_channels, _settings, marker, this, _logger);

            _nodeReassignment = new ReassignFromDormantNodes(marker);
        }

        public void RescheduleOutgoingRecovery()
        {
            _worker.Post(_outgoingMessages);
        }

        public void RescheduleIncomingRecovery()
        {
            _worker.Post(_incomingMessages);
        }


        public void Dispose()
        {
            _connection?.Dispose();
            _scheduledJobTimer?.Dispose();
            _nodeReassignmentTimer?.Dispose();
        }

        private async Task processAction(IMessagingAction action)
        {
            var tx = _connection.BeginTransaction();

            var session = _store.OpenSession(new SessionOptions
            {
                Connection = _connection,
                Transaction = tx,
                Tracking = DocumentTracking.None
            });

            try
            {
                await action.Execute(session);


            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
            finally
            {
                // TODO -- what if this blows up? Try to restart the connection again?
                if (!tx.IsCompleted)
                {
                    await tx.RollbackAsync();
                }

                session.Dispose();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _store.Tenancy.Default.CreateConnection();


            await _connection.OpenAsync(_settings.Cancellation);

            await retrieveLockForThisNode();

            _scheduledJobTimer = new Timer(s =>
            {
                _worker.Post(_scheduledJobs);
                _worker.Post(_incomingMessages);
                _worker.Post(_outgoingMessages);

            }, _settings, _settings.FirstScheduledJobExecution, _settings.ScheduledJobPollingTime);

            _nodeReassignmentTimer = new Timer(s =>
            {
                _worker.Post(_nodeReassignment);


            }, _settings, _settings.FirstNodeReassignmentExecution, _settings.NodeReassignmentPollingTime);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _worker.Complete();

            await _worker.Completion;

            await _connection.CreateCommand().Sql("SELECT pg_advisory_unlock(:id)")
                .Sql("SELECT pg_advisory_lock(:id)")
                .With("id", _settings.UniqueNodeId, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync(CancellationToken.None);

            _connection.Close();
            _connection.Dispose();
            _connection = null;


        }


        private Task retrieveLockForThisNode()
        {
            return _connection
                .CreateCommand()
                .Sql("SELECT pg_advisory_lock(:id)")
                .With("id", _settings.UniqueNodeId, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync(_settings.Cancellation);
        }

    }
}
