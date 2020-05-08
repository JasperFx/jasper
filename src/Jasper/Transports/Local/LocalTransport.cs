using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Transports.Local
{
    public class LocalTransport : ITransport
    {
        private readonly Cache<string, LocalQueueSettings> _queues = new Cache<string, LocalQueueSettings>(name =>
            new LocalQueueSettings(name));

        private ImHashMap<string, ISendingAgent> _agents = ImHashMap<string, ISendingAgent>.Empty;

        public LocalTransport()
        {
            _queues.FillDefault(TransportConstants.Default);
            _queues.FillDefault(TransportConstants.Replies);

            _queues[TransportConstants.Durable].IsDurable = true;
        }

        public IEnumerable<LocalQueueSettings> AllQueues()
        {
            return _queues;
        }

        public Endpoint ReplyEndpoint()
        {
            return _queues[TransportConstants.Replies];
        }

        public void Dispose()
        {
            // Nothing really
        }

        public IEnumerable<Endpoint> Endpoints()
        {
            return _queues;
        }

        public void Initialize(IMessagingRoot root)
        {
            // Nothing
        }


        public ICollection<string> Protocols { get; } = new []{ TransportConstants.Local };

        void ITransport.StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var queue in _queues) addQueue(root, runtime, queue);
        }

        void ITransport.StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            // Nothing
        }


        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            return findByUri(uri);
        }

        public Endpoint TryGetEndpoint(Uri uri)
        {
            var queueName = QueueName(uri);
            return _queues.TryFind(queueName, out var settings) ? settings : null;
        }


        Endpoint ITransport.ListenTo(Uri uri)
        {
            return findByUri(uri);
        }

        private ISendingAgent addQueue(IMessagingRoot root, ITransportRuntime runtime, LocalQueueSettings queue)
        {
            queue.Agent = buildAgent(queue, root);
            _agents = _agents.AddOrUpdate(queue.Name, buildAgent(queue, root));

            runtime.AddSendingAgent(buildAgent(queue, root));
            runtime.AddSubscriber(queue);

            return queue.Agent;
        }

        private ISendingAgent buildAgent(LocalQueueSettings queue, IMessagingRoot root)
        {
            return queue.IsDurable
                ? (ISendingAgent) new DurableLocalSendingAgent(queue, root.Pipeline, root.Settings, root.Persistence,
                    root.TransportLogger, root.Serialization, root.MessageLogger)
                : new LightweightLocalSendingAgent(queue, root.TransportLogger, root.Pipeline, root.Settings,
                    root.MessageLogger);
        }

        private LocalQueueSettings findByUri(Uri uri)
        {
            var queueName = QueueName(uri);
            var settings = _queues[queueName];

            if (uri.IsDurable()) settings.IsDurable = true;

            return settings;
        }

        /// <summary>
        /// Retrieves a local queue by name
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public LocalQueueSettings QueueFor(string queueName)
        {
            return _queues[queueName.ToLowerInvariant()];
        }

        public static string QueueName(Uri uri)
        {
            if (uri == null) return null;

            if (uri == TransportConstants.LocalUri) return TransportConstants.Default;

            if (uri == TransportConstants.DurableLocalUri) return TransportConstants.Durable;

            if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable) return uri.Host;

            var lastSegment = uri.Segments.Skip(1).LastOrDefault();
            if (lastSegment == TransportConstants.Durable) return TransportConstants.Default;

            return lastSegment ?? TransportConstants.Default;
        }

        public static Uri AtQueue(Uri uri, string queueName)
        {
            if (queueName.IsEmpty()) return uri;

            if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable)
                return new Uri("local://" + queueName);

            return new Uri(uri, queueName);
        }

        internal ISendingAgent AddSenderForDestination(Uri uri, IMessagingRoot root, TransportRuntime runtime)
        {
            var queueName = QueueName(uri);
            var queue = _queues[queueName];

            if (uri.IsDurable())
            {
                queue.IsDurable = true;
            }

            return addQueue(root, runtime, queue);
        }

    }
}
