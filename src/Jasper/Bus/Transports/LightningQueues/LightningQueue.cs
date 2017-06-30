using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Lmdb;
using Jasper.Bus.Queues.Storage;
using Jasper.Bus.Runtime;
using LightningDB;

namespace Jasper.Bus.Transports.LightningQueues
{
    public class LightningQueue : IDisposable
    {
        public static readonly string ErrorQueueName = "errors";

        public int Port { get; }
        public bool Persistent { get; }
        private readonly Queue _queue;
        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        public LightningQueue(
            int port,
            bool persistent,
            LightningQueueSettings settings,
            ITransportLogger[] loggers)
        {
            Port = port;
            Persistent = persistent;
            var queueConfiguration = new QueueConfiguration()
                .ReceiveMessagesAt(new IPEndPoint(IPAddress.Any, port))
                .ScheduleQueueWith(TaskPoolScheduler.Default)
                .LogWith(new LightningQueueLoggingAdapter(TransportLogger.Combine(loggers)));

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

            _queue?.Store.Dispose();
            _queue?.Dispose();
        }

        public void Send(Envelope envelope, Uri destination, string subQueue)
        {
            var messagePayload = new Envelope
            {
                Id = MessageId.GenerateRandom(),
                Data = envelope.Data,
                Headers = envelope.Headers,
                SentAt = DateTime.UtcNow,
                Destination = destination,
                Queue = subQueue,
            };

            //TODO Maybe expose something to modify transport specific payloads?
            messagePayload.TranslateHeaders();

            _queue.Send(messagePayload);
        }

        public void ListenForMessages(string subQueue, IReceiver receiver)
        {
            var disposable = _queue.Receive(subQueue).Subscribe(message =>
            {
                Task.Run(() =>
                {
                    receiver.Receive(message.Message.Data, message.Message.Headers,
                        new TransactionCallback(message.QueueContext, message.Message));
                });
            });

            _subscriptions.Add(disposable);
        }
    }
}
