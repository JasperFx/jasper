using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jasper.Testing.Messaging
{
    public class MockMessagingRoot : IMessagingRoot
    {
        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;
        public IScheduledJobProcessor ScheduledJobs { get; } = Substitute.For<IScheduledJobProcessor>();
        public IMessageRouter Router { get; } = Substitute.For<IMessageRouter>();
        public IWorkerQueue Workers { get; } = Substitute.For<IWorkerQueue>();
        public IHandlerPipeline Pipeline { get; } = Substitute.For<IHandlerPipeline>();
        public IMessageLogger Logger { get; } = new MessageLogger(new LoggerFactory(), new NulloMetrics());
        public MessagingSerializationGraph Serialization { get; } = MessagingSerializationGraph.Basic();
        public JasperOptions Options { get; } = new JasperOptions();

        public ITransport[] Transports { get; } =
            {Substitute.For<ITransport>(), Substitute.For<ITransport>(), Substitute.For<ITransport>()};

        public ListeningStatus ListeningStatus
        {
            get => _listeningStatus;
            set
            {
                _listeningStatus = value;


                foreach (var transport in Transports) transport.ListeningStatus = value;
            }
        }

        public ISubscriberGraph Subscribers { get; } = Substitute.For<ISubscriberGraph>();

        public IMessageContext NewContext()
        {
            return new MessageContext(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void ApplyMessageTypeSpecificRules(Envelope envelope)
        {
        }

        public virtual bool ShouldBeDurable(Type messageType)
        {
            return false;
        }

        public ISendingAgent BuildDurableSendingAgent(Uri destination, ISender sender)
        {
            throw new NotImplementedException();
        }

        public ISendingAgent BuildDurableLoopbackAgent(Uri destination)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IListener BuildDurableListener(IListeningAgent agent)
        {
            throw new NotImplementedException();
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return new MessageContext(this, envelope);
        }

        public IEnvelopePersistence Persistence { get; } = Substitute.For<IEnvelopePersistence>();

        public HandlerGraph Handlers { get; } = new HandlerGraph();
    }
}
