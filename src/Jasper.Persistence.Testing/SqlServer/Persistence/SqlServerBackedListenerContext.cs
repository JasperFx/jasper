using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Logging;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Util;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class SqlServerBackedListenerContext : SqlServerContext
    {
        protected readonly IList<Envelope> theEnvelopes = new List<Envelope>();
        protected readonly Uri theUri = "tcp://localhost:1111".ToUri();
        protected SqlServerSettings mssqlSettings;
        protected SqlServerEnvelopePersistence thePersistence;
        protected AdvancedSettings theSettings;
        internal DurableReceiver theReceiver;
        private IHandlerPipeline thePipeline = Substitute.For<IHandlerPipeline>();


        public SqlServerBackedListenerContext()
        {
            theSettings = new AdvancedSettings(null);

            mssqlSettings = new SqlServerSettings
            {
                ConnectionString = Servers.SqlServerConnectionString
            };

            thePersistence = new SqlServerEnvelopePersistence(mssqlSettings, theSettings, new NullLogger<SqlServerEnvelopePersistence>());

            var runtime = Substitute.For<IJasperRuntime>();
            runtime.Persistence.Returns(thePersistence);
            runtime.Pipeline.Returns(thePipeline);
            runtime.Advanced.Returns(theSettings);


            theReceiver = new DurableReceiver(new LocalQueueSettings("temp"), runtime);
        }

        protected Envelope notScheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                MessageType = "foo",
                ContentType = EnvelopeConstants.JsonContentType
            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                ScheduledTime = DateTimeOffset.Now.Add(1.Hours()),
                MessageType = "foo",
                ContentType = EnvelopeConstants.JsonContentType
            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledButExpiredEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                ScheduledTime = DateTimeOffset.Now.Add(-1.Hours()),
                ContentType = EnvelopeConstants.JsonContentType,
                MessageType = "foo"

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected async Task<IReadOnlyList<Envelope>> afterReceivingTheEnvelopes()
        {
            var listener = Substitute.For<IListener>();
            listener.Address.Returns(theUri);
            await theReceiver.ProcessReceivedMessagesAsync(DateTimeOffset.Now, listener, theEnvelopes.ToArray());

            return await thePersistence.Admin.AllIncomingAsync();
        }

        protected void assertEnvelopeWasEnqueued(Envelope envelope)
        {
            thePipeline.Received().InvokeAsync(envelope, theReceiver);
        }

        protected void assertEnvelopeWasNotEnqueued(Envelope envelope)
        {
            thePipeline.DidNotReceive().InvokeAsync(envelope, theReceiver);
        }
    }
}
