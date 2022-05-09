﻿using System;
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
using Newtonsoft.Json;
using NSubstitute;
using ExecutionContext = Jasper.Runtime.ExecutionContext;

namespace Jasper.Testing.Runtime
{

    public class MockJasperRuntime : IJasperRuntime
    {
        public IScheduledJobProcessor ScheduledJobs { get; } = Substitute.For<IScheduledJobProcessor>();
        public IEnvelopeRouter Router { get; } = Substitute.For<IEnvelopeRouter>();
        public IHandlerPipeline Pipeline { get; } = Substitute.For<IHandlerPipeline>();
        public IMessageLogger MessageLogger { get; } = Substitute.For<IMessageLogger>();
        public JasperOptions Options { get; } = new JasperOptions();

        public ITransport[] Transports { get; } =
            {Substitute.For<ITransport>(), Substitute.For<ITransport>(), Substitute.For<ITransport>()};

        public IAcknowledgementSender Acknowledgements { get; } = Substitute.For<IAcknowledgementSender>();
        public IJasperEndpoints Endpoints { get; } = Substitute.For<IJasperEndpoints>();

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

        public IEnumerable<ISubscriber> Subscribers => SubscriberDictionary.Values;

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

        public IListeningWorkerQueue BuildDurableListener(IListener agent)
        {
            throw new NotImplementedException();
        }

        public IEnvelopePersistence Persistence { get; } = Substitute.For<IEnvelopePersistence>();
        public ILogger Logger { get; } = Substitute.For<ILogger>();

        public HandlerGraph Handlers { get; } = new HandlerGraph();

        public readonly Dictionary<Uri, ISubscriber> SubscriberDictionary = new Dictionary<Uri,ISubscriber>();

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

        public void AddSendingAgent(ISendingAgent sendingAgent)
        {
            throw new NotImplementedException();
        }

        public void AddSubscriber(ISubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public ISendingAgent AgentForLocalQueue(string queueName)
        {
            throw new NotImplementedException();
        }

        public Endpoint? EndpointFor(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
