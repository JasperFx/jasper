using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Concurrency;
using JasperBus.Configuration;
using LightningDB;
using LightningQueues;
using LightningQueues.Storage;
using LightningQueues.Storage.LMDB;

namespace JasperBus.Transports.LightningQueues
{
    public class LightningQueue : IDisposable
    {
        public static readonly string ErrorQueueName = "errors";

        public int Port { get; }
        public bool Persistent { get; }
        private readonly Queue _queue;
        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        public LightningQueue(int port, bool persistent, LightningQueueSettings settings)
        {
            Port = port;
            Persistent = persistent;
            var queueConfiguration = new QueueConfiguration()
                .ReceiveMessagesAt(new IPEndPoint(IPAddress.Any, port))
                .ScheduleQueueWith(TaskPoolScheduler.Default);

            // TODO -- bring through whatever the Jasper logging abstraction ends up being here.
            //.LogWith(new FubuLoggingAdapter(_logger));

            if (persistent)
            {
                queueConfiguration.StoreWithLmdb(settings.QueuePath + "." + port, new EnvironmentConfiguration
                {
                    MaxDatabases = settings.MaxDatabases, MapSize = settings.MapSize
                });
            }
            else
            {
                queueConfiguration.UseNoStorage();
            }

            _queue = queueConfiguration.BuildQueue();
        }

        public void Start(ChannelGraph channels, IEnumerable<ChannelNode> nodes)
        {
            try
            {
                _queue.CreateQueue(ErrorQueueName);

                foreach (var node in nodes)
                {
                    var lqUri = node.Uri.ToLightningUri();

                    _queue.CreateQueue(lqUri.QueueName);
                }

                _queue.Start();
            }
            catch (Exception e)
            {
                throw new LightningQueueTransportException(new IPEndPoint(IPAddress.Any, Port), e);
            }
        }


        public void ClearAll()
        {
            _queue.Store.ClearAllStorage();
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();

            _queue?.Dispose();
        }

        public void Send(byte[] data, IDictionary<string, string> headers, Uri destination, string subQueue)
        {
            var messagePayload = new OutgoingMessage
            {
                Id = MessageId.GenerateRandom(),
                Data = data,
                Headers = headers,
                SentAt = DateTime.UtcNow,
                Destination = destination,
                Queue = subQueue,
            };

            //TODO Maybe expose something to modify transport specific payloads?
            messagePayload.TranslateHeaders();

            _queue.Send(messagePayload);
        }
    }
}