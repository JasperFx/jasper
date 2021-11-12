using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Baseline;
using Baseline.ImTools;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports.Local;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Transports
{
    public class TransportRuntime : ITransportRuntime
    {

        private readonly IList<IDisposable> _disposables = new List<IDisposable>();
        private readonly IList<ISubscriber> _subscribers = new List<ISubscriber>();

        private readonly object _channelLock = new object();

        private ImHashMap<Uri, ISendingAgent> _senders = ImHashMap<Uri, ISendingAgent>.Empty;


        private readonly IMessagingRoot _root;
        private TransportCollection _transports;

        public TransportRuntime(IMessagingRoot root)
        {
            _root = root;
            _transports = root.Options.Transports;
        }

        public void Initialize()
        {
            foreach (var transport in _transports)
            {
                transport.Initialize(_root);
                foreach (var endpoint in transport.Endpoints())
                {
                    endpoint.Root = _root; // necessary to locate serialization
                }
            }

            foreach (var transport in _transports)
            {
                transport.StartSenders(_root, this);
            }

            foreach (var transport in _transports)
            {
                transport.StartListeners(_root, this);
            }

            foreach (var subscriber in _transports.Subscribers)
            {
                _subscribers.Fill(subscriber);
            }
        }

        public ISendingAgent AddSubscriber(Uri replyUri, ISender sender, Endpoint endpoint)
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
                throw new TransportEndpointException(sender.Destination, "Could not build sending sendingAgent. See inner exception.", e);
            }
        }

        private ISendingAgent buildSendingAgent(ISender sender, Endpoint endpoint)
        {
            // This is for the stub transport in the Storyteller specs
            if (sender is ISendingAgent a) return a;

            switch (endpoint.Mode)
            {
                case EndpointMode.Durable:
                    return new DurableSendingAgent(sender, _root.Settings, _root.TransportLogger, _root.MessageLogger,
                        _root.Persistence, endpoint);

                case EndpointMode.BufferedInMemory:
                    return new LightweightSendingAgent(_root.TransportLogger, _root.MessageLogger, sender, _root.Settings, endpoint);

                case EndpointMode.Inline:
                    return new InlineSendingAgent(sender, endpoint, _root.MessageLogger, _root.Settings);
            }

            throw new InvalidOperationException();
        }

        public void AddSendingAgent(ISendingAgent sendingAgent)
        {
            _senders = _senders.AddOrUpdate(sendingAgent.Destination, sendingAgent);
        }

        public void AddSubscriber(ISubscriber subscriber)
        {
            _subscribers.Fill(subscriber);
        }

        private ImHashMap<string, ISendingAgent> _localSenders = ImHashMap<string, ISendingAgent>.Empty;

        public ISendingAgent AgentForLocalQueue(string queueName)
        {
            queueName = queueName ?? TransportConstants.Default;
            if (_localSenders.TryFind(queueName, out var agent))
            {
                return agent;
            }

            agent = GetOrBuildSendingAgent($"local://{queueName}".ToUri());
            _localSenders = _localSenders.AddOrUpdate(queueName, agent);

            return agent;
        }



        public ISendingAgent GetOrBuildSendingAgent(Uri address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            if (_senders.TryFind(address, out var agent)) return agent;

            lock (_channelLock)
            {
                return !_senders.TryFind(address, out agent)
                    ? buildSendingAgent(address)
                    : agent;
            }
        }

        private ISendingAgent buildSendingAgent(Uri uri)
        {
            var transport = _transports.TransportForScheme(uri.Scheme);
            if (transport == null)
            {
                throw new InvalidOperationException($"There is no known transport type that can send to the Destination {uri}");
            }

            if (uri.Scheme == TransportConstants.Local)
            {
                var local = (LocalTransport)transport;
                var agent = local.AddSenderForDestination(uri, _root, this);
                agent.Endpoint.Root = _root; // This is important for serialization

                AddSendingAgent(agent);

                return agent;
            }
            else
            {
                var endpoint = transport.GetOrCreateEndpoint(uri);
                endpoint.Root ??= _root; // This is important for serialization
                return endpoint.StartSending(_root, _root.Runtime, transport.ReplyEndpoint()?.ReplyUri());
            }


        }

        public void AddListener(IListener listener, Endpoint settings)
        {

            IDisposable worker = null;
            switch (settings.Mode)
            {
                case EndpointMode.Durable:
                    worker = new DurableWorkerQueue(settings, _root.Pipeline, _root.Settings, _root.Persistence,
                        _root.TransportLogger);
                    break;

                case EndpointMode.BufferedInMemory:
                    worker = new LightweightWorkerQueue(settings, _root.TransportLogger, _root.Pipeline, _root.Settings);
                    break;

                case EndpointMode.Inline:
                    worker = new InlineWorkerQueue(_root.Pipeline, _root.TransportLogger, listener, _root.Settings);
                    break;
            }

            if (worker is IWorkerQueue q) q.StartListening(listener);
            _disposables.Add(worker);
        }

        public Task Stop()
        {
            // TODO -- this needs to be draining the senders and listeners
            throw new NotImplementedException();
        }

        public ISubscriber[] FindSubscribersForMessageType(Type messageType)
        {
            return _subscribers
                .Where(x => x.ShouldSendMessage(messageType))
                .ToArray();
        }


        public ISendingAgent[] FindLocalSubscribers(Type messageType)
        {
            return _subscribers
                .OfType<LocalQueueSettings>()
                .Where(x => x.ShouldSendMessage(messageType))
                .Select(x => x.Agent)
                .ToArray();

        }

        public ITopicRouter[] FindTopicRoutersForMessageType(Type messageType)
        {
            var routers = FindSubscribersForMessageType(messageType).OfType<ITopicRouter>().ToArray();
            return routers.Any() ? routers : _subscribers.OfType<ITopicRouter>().ToArray();
        }

        public IEnumerable<Endpoint> AllEndpoints()
        {
            return _transports.SelectMany(x => x.Endpoints());
        }

        public Endpoint EndpointFor(Uri uri)
        {
            return AllEndpoints().FirstOrDefault(x => x.Uri == uri);
        }

        public void Dispose()
        {
            foreach (var kv in _senders.Enumerate())
            {
                kv.Value.SafeDispose();
            }

            foreach (var listener in _disposables)
            {
                listener.SafeDispose();
            }

            foreach (var transport in _transports.OfType<IDisposable>())
            {
                transport.Dispose();
            }
        }

    }
}
