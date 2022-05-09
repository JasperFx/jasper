using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Runtime;

public partial class JasperRuntime : IJasperEndpoints
{
    private readonly object _channelLock = new();
    private readonly IList<object> _disposables = new List<object>();
    private readonly List<ISubscriber> _subscribers = new();

    private ImHashMap<string, ISendingAgent> _localSenders = ImHashMap<string, ISendingAgent>.Empty;

    private ImHashMap<Uri?, ISendingAgent> _senders = ImHashMap<Uri, ISendingAgent>.Empty!;

    public IEnumerable<ISubscriber> Subscribers => _subscribers;

    public ISendingAgent AddSubscriber(Uri? replyUri, ISender sender, Endpoint endpoint)
    {
        try
        {
            var agent = buildSendingAgent(sender, endpoint);

            agent.ReplyUri = replyUri;

            endpoint.Agent = agent;

            if (sender is ISenderRequiresCallback senderRequiringCallback && agent is ISenderCallback callbackAgent)
            {
                senderRequiringCallback.RegisterCallback(callbackAgent);
            }

            AddSendingAgent(agent);
            AddSubscriber(endpoint);

            return agent;
        }
        catch (Exception e)
        {
            throw new TransportEndpointException(sender.Destination,
                "Could not build sending sendingAgent. See inner exception.", e);
        }
    }

    public void AddSendingAgent(ISendingAgent sendingAgent)
    {
        _senders = _senders.AddOrUpdate(sendingAgent.Destination, sendingAgent);
    }

    public void AddSubscriber(ISubscriber subscriber)
    {
        _subscribers.Fill(subscriber);
    }


    public ISendingAgent GetOrBuildSendingAgent(Uri address)
    {
        if (address == null)
        {
            throw new ArgumentNullException(nameof(address));
        }

        if (_senders.TryFind(address, out var agent))
        {
            return agent;
        }

        lock (_channelLock)
        {
            return !_senders.TryFind(address, out agent)
                ? buildSendingAgent(address)
                : agent;
        }
    }

    public void AddListener(IListener listener, Endpoint settings)
    {
        object? worker = settings.Mode switch
        {
            EndpointMode.Durable => new DurableWorkerQueue(settings, Pipeline, Advanced,
                Persistence, Logger),
            EndpointMode.BufferedInMemory => new LightweightWorkerQueue(settings, Logger, Pipeline,
                Advanced),
            EndpointMode.Inline => new InlineWorkerQueue(Pipeline, Logger, listener, Advanced),
            _ => null
        };

        if (worker is IWorkerQueue q)
        {
            q.StartListening(listener);
        }

        _disposables.Add(worker!);
    }

    public Endpoint? EndpointFor(Uri uri)
    {
        return endpoints().FirstOrDefault(x => x.Uri == uri);
    }

    private ISendingAgent buildSendingAgent(ISender sender, Endpoint endpoint)
    {
        // This is for the stub transport in the Storyteller specs
        if (sender is ISendingAgent a)
        {
            return a;
        }

        switch (endpoint.Mode)
        {
            case EndpointMode.Durable:
                return new DurableSendingAgent(sender, Advanced, Logger, MessageLogger,
                    Persistence, endpoint);

            case EndpointMode.BufferedInMemory:
                return new LightweightSendingAgent(Logger, MessageLogger, sender, Advanced, endpoint);

            case EndpointMode.Inline:
                return new InlineSendingAgent(sender, endpoint, MessageLogger, Advanced);
        }

        throw new InvalidOperationException();
    }

    public ISendingAgent AgentForLocalQueue(string queueName)
    {
        if (_localSenders.TryFind(queueName, out var agent))
        {
            return agent;
        }

        agent = GetOrBuildSendingAgent($"local://{queueName}".ToUri());
        _localSenders = _localSenders.AddOrUpdate(queueName, agent);

        return agent;
    }

    private ISendingAgent buildSendingAgent(Uri uri)
    {
        var transport = Options.TransportForScheme(uri.Scheme);
        if (transport == null)
        {
            throw new InvalidOperationException(
                $"There is no known transport type that can send to the Destination {uri}");
        }

        if (uri.Scheme == TransportConstants.Local)
        {
            var local = (LocalTransport)transport;
            var agent = local.AddSenderForDestination(uri, this);
            agent.Endpoint.Root = this; // This is important for serialization

            AddSendingAgent(agent);

            return agent;
        }

        var endpoint = transport.GetOrCreateEndpoint(uri);
        endpoint.Root ??= this; // This is important for serialization
        return endpoint.StartSending(this, transport.ReplyEndpoint()?.CorrectedUriForReplies());
    }

    private IEnumerable<Endpoint> endpoints()
    {
        return Options.SelectMany(x => x.Endpoints());
    }
}
