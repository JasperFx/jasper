using System;
using System.Collections.Generic;
using System.Linq;
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
        public IHandlerPipeline Pipeline { get; } = Substitute.For<IHandlerPipeline>();
        public IMessageLogger MessageLogger { get; } = new MessageLogger(new LoggerFactory(), new NulloMetrics());
        public MessagingSerializationGraph Serialization { get; } = MessagingSerializationGraph.Basic();
        public JasperOptions Options { get; } = new JasperOptions();

        public ITransport[] Transports { get; } =
            {Substitute.For<ITransport>(), Substitute.For<ITransport>(), Substitute.For<ITransport>()};

        public IMessageContext NewContext()
        {
            return new MessageContext(this);
        }

        public AdvancedSettings Settings { get; } = new AdvancedSettings();
        public ITransportRuntime Runtime { get; } = Substitute.For<ITransportRuntime>();


        public void AddListener(Endpoint endpoint, IListener agent)
        {

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IListeningWorkerQueue BuildDurableListener(IListener agent)
        {
            throw new NotImplementedException();
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return new MessageContext(this, envelope);
        }

        public IEnvelopePersistence Persistence { get; } = Substitute.For<IEnvelopePersistence>();
        public ITransportLogger TransportLogger { get; } = Substitute.For<ITransportLogger>();

        public HandlerGraph Handlers { get; } = new HandlerGraph();

        public readonly Dictionary<Uri, Subscriber> Subscribers = new Dictionary<Uri,Subscriber>();

        public ISendingAgent GetOrBuild(Uri address)
        {
            throw new NotSupportedException();
//            if (Subscribers.TryGetValue(address, out var subscriber))
//            {
//                return subscriber;
//            }
//
//            return null;
        }

        public Subscriber[] AllKnown()
        {
            return Subscribers.Values.ToArray();
        }

        public void AddSubscriber(Subscriber subscriber)
        {
            throw new NotImplementedException();
        }
    }
}
