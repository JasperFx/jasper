using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Tracking;
using TestingSupport;
using TestingSupport.ErrorHandling;

namespace Jasper.Testing.ErrorHandling
{
    public class ErrorHandlingContext
    {
        protected readonly JasperOptions theOptions = new JasperOptions();
        protected readonly ErrorCausingMessage theMessage = new ErrorCausingMessage();
        private ITrackedSession _session;

        public ErrorHandlingContext()
        {
            theOptions.Handlers.DisableConventionalDiscovery()
                .Discovery(x => x.IncludeType<ErrorCausingMessageHandler>());
        }

        protected void throwOnAttempt<T>(int attempt) where T : Exception, new()
        {
            theMessage.Errors.Add(attempt, new T());
        }

        protected async Task<EnvelopeRecord> afterProcessingIsComplete()
        {
            using (var host = JasperHost.For(theOptions))
            {
                _session = await host
                    .TrackActivity()
                    .DoNotAssertOnExceptionsDetected()
                    .SendMessageAndWaitAsync(theMessage);

                return _session.AllRecordsInOrder().LastOrDefault(x =>
                    x.EventType == EventType.MessageSucceeded || x.EventType == EventType.MovedToErrorQueue);


            }
        }

        protected async Task shouldSucceedOnAttempt(int attempt)
        {
            using (var host = JasperHost.For(theOptions))
            {
                var session = await host
                    .TrackActivity()
                    .DoNotAssertOnExceptionsDetected()
                    .SendMessageAndWaitAsync(theMessage);

                var record = session.AllRecordsInOrder().LastOrDefault(x =>
                    x.EventType == EventType.MessageSucceeded || x.EventType == EventType.MovedToErrorQueue);

                if (record == null) throw new Exception("No ending activity detected");

                if (record.EventType == EventType.MessageSucceeded && record.AttemptNumber == attempt)
                {
                    return;
                }

                var writer = new StringWriter();

                writer.WriteLine($"Actual ending was '{record.EventType}' on attempt {record.AttemptNumber}");
                foreach (var envelopeRecord in session.AllRecordsInOrder())
                {
                    writer.WriteLine(envelopeRecord);
                    if (envelopeRecord.Exception != null)
                    {
                        writer.WriteLine(envelopeRecord.Exception.Message);
                    }
                }

                throw new Exception(writer.ToString());
            }
        }

        protected async Task shouldMoveToErrorQueueOnAttempt(int attempt)
        {
            using (var host = JasperHost.For(theOptions))
            {
                var session = await host
                    .TrackActivity()
                    .DoNotAssertOnExceptionsDetected()
                    .SendMessageAndWaitAsync(theMessage);

                var record = session.AllRecordsInOrder().LastOrDefault(x =>
                    x.EventType == EventType.MessageSucceeded || x.EventType == EventType.MovedToErrorQueue);

                if (record == null) throw new Exception("No ending activity detected");

                if (record.EventType == EventType.MovedToErrorQueue && record.AttemptNumber == attempt)
                {
                    return;
                }

                var writer = new StringWriter();

                writer.WriteLine($"Actual ending was '{record.EventType}' on attempt {record.AttemptNumber}");
                foreach (var envelopeRecord in session.AllRecordsInOrder())
                {
                    writer.WriteLine(envelopeRecord);
                    if (envelopeRecord.Exception != null)
                    {
                        writer.WriteLine(envelopeRecord.Exception.Message);
                    }
                }

                throw new Exception(writer.ToString());
            }
        }




    }
}
