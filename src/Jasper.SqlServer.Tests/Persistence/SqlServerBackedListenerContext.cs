using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Schema;
using Jasper.Util;
using NSubstitute;
using Shouldly;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class SqlServerBackedListenerContext
    {
        protected readonly Uri theUri = "tcp://localhost:1111".ToUri();
        protected IWorkerQueue theWorkerQueue;
        protected MessagingSettings theSettings;
        protected DurableListener theListener;

        protected readonly IList<Envelope> theEnvelopes = new List<Envelope>();
        protected SqlServerEnvelopePersistor thePersistor;
        protected EnvelopeRetries retries;
        protected SqlServerSettings mssqlSettings;


        public SqlServerBackedListenerContext()
        {
            new SchemaLoader(ConnectionSource.ConnectionString).RecreateAll();

            theWorkerQueue = Substitute.For<IWorkerQueue>();

            theSettings = new MessagingSettings();

            mssqlSettings = new SqlServerSettings
            {
                ConnectionString = ConnectionSource.ConnectionString
            };

            thePersistor = new SqlServerEnvelopePersistor(mssqlSettings);

            retries = new EnvelopeRetries(thePersistor, TransportLogger.Empty(), theSettings);


            theListener = new DurableListener(
                Substitute.For<IListeningAgent>(),
                theWorkerQueue,
                TransportLogger.Empty(), theSettings, retries, thePersistor);
        }

        protected Envelope notScheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[]{1,2,3,4}

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[]{1,2,3,4},
                ExecutionTime = DateTime.UtcNow.Add(1.Hours())

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledButExpiredEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[]{1,2,3,4},
                ExecutionTime = DateTime.UtcNow.Add(-1.Hours())

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected async Task<IReadOnlyList<Envelope>> afterReceivingTheEnvelopes()
        {
            var status = await theListener.Received(theUri, theEnvelopes.ToArray());

            status.ShouldBe(ReceivedStatus.Successful);

            return thePersistor.AllIncomingEnvelopes();
        }

        protected void assertEnvelopeWasEnqueued(Envelope envelope)
        {
            theWorkerQueue.Received().Enqueue(envelope);
        }

        protected void assertEnvelopeWasNotEnqueued(Envelope envelope)
        {
            theWorkerQueue.DidNotReceive().Enqueue(envelope);
        }



    }
}
