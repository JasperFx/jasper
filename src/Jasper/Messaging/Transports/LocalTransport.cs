using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    // TODO -- UT this beast
    public class LocalTransport : ITransport
    {
        private readonly Cache<string, LocalQueueSettings> _queues = new Cache<string, LocalQueueSettings>(name =>
            new LocalQueueSettings(name)
            {
                Uri = new Uri($"local://{name}")
            });

        private ImHashMap<string, ISendingAgent> _agents = ImHashMap<string, ISendingAgent>.Empty;

        public LocalTransport()
        {
            _queues.FillDefault(TransportConstants.Retries);
            _queues.FillDefault(TransportConstants.Default);
            _queues.FillDefault(TransportConstants.Replies);
        }

        public void Dispose()
        {
            // Nothing really
        }

        public string Protocol { get; } = TransportConstants.Local;

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var queue in _queues) addQueue(root, runtime, queue);
        }

        public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            // Nothing
        }

        public ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
        {
            throw new NotSupportedException();
        }

        public void Subscribe(Subscription subscription)
        {
            RetrieveQueueByUri(subscription.Uri).Subscriptions.Add(subscription);
        }


        public Uri ReplyUri => TransportConstants.RepliesUri;

        public IListenerSettings ListenTo(Uri uri)
        {
            return RetrieveQueueByUri(uri);
        }

        public ISendingAgent AddSenderForDestination(string queueName, IMessagingRoot root, ITransportRuntime runtime)
        {
            var queue = _queues[queueName];
            return addQueue(root, runtime, queue);
        }

        private ISendingAgent addQueue(IMessagingRoot root, ITransportRuntime runtime, LocalQueueSettings queue)
        {
            var agent = buildAgent(queue, root);
            _agents = _agents.AddOrUpdate(queue.Name, agent);

            runtime.AddSubscriber(agent, queue.Subscriptions.ToArray());

            return agent;
        }

        private ISendingAgent buildAgent(LocalQueueSettings queue, IMessagingRoot root)
        {
            return queue.IsDurable
                ? (ISendingAgent) new DurableLocalSendingAgent(queue, root.Pipeline, root.Settings, root.Persistence,
                    root.TransportLogger, root.Serialization, root.MessageLogger)
                : new LightweightLocalSendingAgent(queue, root.TransportLogger, root.Pipeline, root.Settings,
                    root.MessageLogger);
        }

        public LocalQueueSettings RetrieveQueueByUri(Uri uri)
        {
            var queueName = QueueName(uri);
            var settings = _queues[queueName];

            if (uri.IsDurable()) settings.IsDurable = true;

            return settings;
        }

        public static string QueueName(Uri uri)
        {
            if (uri == null) return null;

            if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable) return uri.Host;

            var lastSegment = uri.Segments.Skip(1).LastOrDefault();
            if (lastSegment == TransportConstants.Durable) return TransportConstants.Default;

            return lastSegment ?? TransportConstants.Default;
        }

        public static Uri AtQueue(Uri uri, string queueName)
        {
            if (queueName.IsEmpty()) return uri;

            if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable)
            {
                return new Uri("local://" + queueName);
            }

            return new Uri(uri, queueName);
        }

        public ISendingAgent AddSenderForDestination(Uri uri, IMessagingRoot root, TransportRuntime runtime)
        {
            var queueName = QueueName(uri);
            return AddSenderForDestination(queueName, root, runtime);
        }
    }

    public class LocalQueueSettings : ListenerSettings
    {
        public LocalQueueSettings(string name)
        {
            Name = name;
        }


        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();
    }
}
