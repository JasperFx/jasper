using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using ImTools;
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

        private readonly IList<IListeningWorkerQueue> _listeners = new List<IListeningWorkerQueue>();
        private readonly IList<ISubscriber> _subscribers = new List<ISubscriber>();

        private readonly object _channelLock = new object();

        private ImTools.ImHashMap<Uri, ISendingAgent> _senders = ImTools.ImHashMap<Uri, ISendingAgent>.Empty;


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
                var agent = endpoint.IsDurable
                    ? (ISendingAgent)new DurableSendingAgent(sender, _root.Settings, _root.TransportLogger, _root.MessageLogger, _root.Persistence, endpoint)
                    : new LightweightSendingAgent(_root.TransportLogger, _root.MessageLogger, sender, _root.Settings, endpoint);

                agent.ReplyUri = replyUri;
                sender.Start((ISenderCallback) agent);

                endpoint.Agent = agent;

                AddSendingAgent(agent);
                AddSubscriber(endpoint);

                return agent;
            }
            catch (Exception e)
            {
                throw new TransportEndpointException(sender.Destination, "Could not build sending sendingAgent. See inner exception.", e);
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

        private ImTools.ImHashMap<string, ISendingAgent> _localSenders = ImTools.ImHashMap<string, ISendingAgent>.Empty;

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

                AddSendingAgent(agent);

                return agent;
            }
            else
            {
                var endpoint = transport.GetOrCreateEndpoint(uri);
                return endpoint.StartSending(_root, _root.Runtime, transport.ReplyEndpoint()?.ReplyUri());
            }


        }

        public void AddListener(IListener listener, Endpoint settings)
        {
            var worker = settings.IsDurable
                ? (IWorkerQueue) new DurableWorkerQueue(settings, _root.Pipeline, _root.Settings, _root.Persistence,
                    _root.TransportLogger)
                : new LightweightWorkerQueue(settings, _root.TransportLogger, _root.Pipeline, _root.Settings);


            _listeners.Add(worker);


            worker.StartListening(listener);
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

        public void Dispose()
        {
            foreach (var kv in _senders.Enumerate())
            {
                kv.Value.SafeDispose();
            }

            foreach (var listener in _listeners)
            {
                listener.SafeDispose();
            }
        }

    }
}
