using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Invocation;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jasper.Testing.Runtime
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
        public CancellationToken Cancellation { get; } = default(CancellationToken);


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

        public void Dispose()
        {
        }
    }
}
