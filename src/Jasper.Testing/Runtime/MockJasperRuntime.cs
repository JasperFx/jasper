using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
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
using Jasper.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using NSubstitute;
using ExecutionContext = Jasper.Runtime.ExecutionContext;

namespace Jasper.Testing.Runtime
{

    public class MockJasperRuntime : IJasperRuntime
    {
        public void ScheduleLocalExecutionInMemory(DateTimeOffset executionTime, Envelope envelope)
        {
            throw new NotSupportedException();
        }

        public IHandlerPipeline Pipeline { get; } = Substitute.For<IHandlerPipeline>();
        public IMessageLogger MessageLogger { get; } = Substitute.For<IMessageLogger>();
        public JasperOptions Options { get; } = new JasperOptions();

        public ISendingAgent CreateSendingAgent(Uri replyUri, ISender sender, Endpoint endpoint)
        {
            throw new NotImplementedException();
        }

        public ISendingAgent GetOrBuildSendingAgent(Uri address, Action<Endpoint> configureNewEndpoint = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IListeningAgent> ActiveListeners()
        {
            throw new NotImplementedException();
        }

        public void AddSendingAgent(ISendingAgent sendingAgent)
        {
            throw new NotImplementedException();
        }

        public Endpoint EndpointFor(Uri uri)
        {
            throw new NotImplementedException();
        }

        public IMessageRouter RoutingFor(Type messageType)
        {
            throw new NotImplementedException();
        }

        public T TryFindExtension<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public bool TryFindMessageType(string? messageTypeName, out Type messageType)
        {
            throw new NotSupportedException();
        }

        public Type DetermineMessageType(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                if (TryFindMessageType(envelope.MessageType, out var messageType))
                {
                    return messageType;
                }

                throw new InvalidOperationException($"Unable to determine a message type for `{envelope.MessageType}`, the known types are: {Handlers.Chains.Select(x => x.MessageType.ToMessageTypeName()).Join(", ")}");
            }

            if (envelope.Message == null) throw new ArgumentNullException(nameof(Envelope.Message));
            return envelope.Message.GetType();
        }

        public void RegisterMessageType(Type messageType)
        {
            throw new NotImplementedException();
        }

        public ISendingAgent AddSubscriber(Uri? replyUri, ISender sender, Endpoint endpoint)
        {
            throw new NotImplementedException();
        }

        public ISendingAgent GetOrBuildSendingAgent(Uri address)
        {
            return Substitute.For<ISendingAgent>();
        }

        public void AddListener(IListener listener, Endpoint settings)
        {
            throw new NotImplementedException();
        }

        public IExecutionContext NewContext()
        {
            return new ExecutionContext(this);
        }

        public AdvancedSettings? Advanced { get; } = new AdvancedSettings(null);
        public CancellationToken Cancellation { get; } = default(CancellationToken);


        public void AddListener(Endpoint endpoint, IListener agent)
        {

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IReceiver BuildDurableListener(IListener agent)
        {
            throw new NotImplementedException();
        }

        public IEnvelopePersistence Persistence { get; } = Substitute.For<IEnvelopePersistence>();
        public ILogger Logger { get; } = Substitute.For<ILogger>();

        public HandlerGraph Handlers { get; } = new HandlerGraph();


        public void Dispose()
        {
        }
    }
}
