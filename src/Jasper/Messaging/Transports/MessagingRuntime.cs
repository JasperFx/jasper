using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImTools;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Local;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{


    public class TransportRuntime : ITransportRuntime
    {

        private readonly IList<IListeningWorkerQueue> _listeners = new List<IListeningWorkerQueue>();
        private readonly IList<Subscriber> _subscribers = new List<Subscriber>();

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
                transport.StartSenders(_root, this);
            }

            foreach (var transport in _transports)
            {
                transport.StartListeners(_root, this);
            }
        }

        public ISendingAgent AddSubscriber(Uri replyUri, ISender sender, Subscription[] subscriptions)
        {
            try
            {
                var agent = sender.Destination.IsDurable()
                    ? (ISendingAgent)new DurableSendingAgent(sender, _root.Settings, _root.TransportLogger, _root.MessageLogger, _root.Persistence)
                    : new LightweightSendingAgent(_root.TransportLogger, _root.MessageLogger, sender, _root.Settings);

                agent.ReplyUri = replyUri;
                sender.Start((ISenderCallback) agent);

                AddSubscriber(agent, subscriptions);

                return agent;
            }
            catch (Exception e)
            {
                throw new TransportEndpointException(sender.Destination, "Could not build sending agent. See inner exception.", e);
            }
        }

        public void AddSubscriber(ISendingAgent agent, Subscription[] subscriptions)
        {
            _senders = _senders.AddOrUpdate(agent.Destination, agent);

            var subscriber = new Subscriber(agent, subscriptions);
            _subscribers.Add(subscriber);
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

                AddSubscriber(agent, new Subscription[0]);

                return agent;
            }
            else
            {
                var endpoint = transport.DetermineEndpoint(uri);
                return endpoint.StartSending(_root, _root.Runtime, transport.ReplyUri);
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

        public ISendingAgent[] FindSubscribers(Type messageType)
        {
            return _subscribers.Where(x => x.ShouldSendMessage(messageType))
                .Select(x => x.Agent)
                .ToArray();
        }



        public void Dispose()
        {
            // TODO -- do stuff
        }

    }
}
