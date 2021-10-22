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
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ExecutionContext = Jasper.Runtime.ExecutionContext;

namespace Jasper.Testing.Runtime
{

    public class MockMessagingRoot : IMessagingRoot
    {
        public IScheduledJobProcessor ScheduledJobs { get; } = Substitute.For<IScheduledJobProcessor>();
        public IEnvelopeRouter Router { get; } = Substitute.For<IEnvelopeRouter>();
        public IHandlerPipeline Pipeline { get; } = Substitute.For<IHandlerPipeline>();
        public IMessageLogger MessageLogger { get; } = Substitute.For<IMessageLogger>();
        public MessagingSerializationGraph Serialization { get; } = MessagingSerializationGraph.Basic();
        public JasperOptions Options { get; } = new JasperOptions();

        public ITransport[] Transports { get; } =
            {Substitute.For<ITransport>(), Substitute.For<ITransport>(), Substitute.For<ITransport>()};

        public IAcknowledgementSender Acknowledgements { get; } = Substitute.For<IAcknowledgementSender>();

        public IExecutionContext NewContext()
        {
            return new ExecutionContext(this);
        }

        public AdvancedSettings Settings { get; } = new AdvancedSettings(null);
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

        public IExecutionContext ContextFor(Envelope envelope)
        {
            return new ExecutionContext(this, envelope, InvocationCallback.Instance);
        }

        public IEnvelopePersistence Persistence { get; } = Substitute.For<IEnvelopePersistence>();
        public ITransportLogger TransportLogger { get; } = Substitute.For<ITransportLogger>();

        public HandlerGraph Handlers { get; } = new HandlerGraph();

        public readonly Dictionary<Uri, ISubscriber> Subscribers = new Dictionary<Uri,ISubscriber>();

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

        public ISubscriber[] AllKnown()
        {
            return Subscribers.Values.ToArray();
        }

        public void AddSubscriber(ISubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
