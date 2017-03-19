using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Runtime.Serialization;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using LightningDB;
using LightningQueues;
using LightningQueues.Logging;
using LightningQueues.Storage;
using LightningQueues.Storage.LMDB;
using StructureMap.TypeRules;

namespace JasperBus.Transports.LightningQueues
{
    public class LightningQueuesTransport : ITransport
    {
        public static string MaxAttemptsHeader = "max-delivery-attempts";
        public static string DeliverByHeader = "deliver-by";


        private readonly ConcurrentDictionary<int, LightningQueue> _queues = new ConcurrentDictionary<int, LightningQueue>();
        private readonly LightningQueueSettings _settings;
        private readonly ConcurrentDictionary<Uri, LightningUri> _uris = new ConcurrentDictionary<Uri, LightningUri>();
        private Uri _replyUri;

        public LightningQueuesTransport(LightningQueueSettings settings)
        {
            _settings = settings;
        }

        public void Dispose()
        {
            _queues.Values.Each(x => x.Dispose());
            _queues.Clear();
        }

        public string Protocol => "lq.tcp";

        public Uri ReplyUriFor(Uri node)
        {
            throw new NotImplementedException();
        }

        private LightningUri lqUriFor(Uri uri)
        {
            return _uris.GetOrAdd(uri, u => new LightningUri(u));
        }

        public void Send(Uri uri, byte[] data, IDictionary<string, string> headers)
        {
            var lqUri = lqUriFor(uri);

        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;

            var replyNode = nodes.FirstOrDefault(x => x.Incoming) ??
                            channels.AddChannelIfMissing(_settings.DefaultReplyUri);

            replyNode.Incoming = true;
            _replyUri = replyNode.Uri.ToLightningUri().Address;


            var groups = nodes.GroupBy(x => x.Uri.Port);

            foreach (var group in groups)
            {
                // TODO -- need to worry about persistence or not here
                var queue = _queues.GetOrAdd(group.Key, key => new LightningQueue(group.Key, true, _settings));
                queue.Start(channels, group);

                foreach (var node in group)
                {
                    node.Sender = new QueueSender(queue);
                }
            }


            foreach (var node in nodes)
            {
                node.Destination = node.Uri.ToLightningUri().Address;
                node.ReplyUri = _replyUri;
            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }
    }

    public class QueueSender : ISender
    {
        private readonly LightningQueue _queue;

        public QueueSender(LightningQueue queue)
        {
            _queue = queue;
        }

        public void Send(byte[] data, IDictionary<string, string> headers)
        {
            // TODO -- pull over from fubu
            throw new NotImplementedException();
        }
    }

    public class FubuLoggingAdapter : ILogger
    {
        public void Debug(string message)
        {

        }

        public void DebugFormat(string message, params object[] args)
        {
        }

        public void DebugFormat(string message, object arg1, object arg2)
        {
        }

        public void DebugFormat(string message, object arg1)
        {
        }

        public void Info(string message)
        {
        }

        public void InfoFormat(string message, params object[] args)
        {
        }

        public void InfoFormat(string message, object arg1, object arg2)
        {
        }

        public void InfoFormat(string message, object arg1)
        {
        }

        public void Error(string message, Exception exception)
        {
        }
    }



    public static class MessageExtensions
    {
        public static Envelope ToEnvelope(this Message message)
        {
            var envelope = new Envelope(message.Headers)
            {
                Data = message.Data
            };

            return envelope;
        }

        public static Message Copy(this Message message)
        {
            var copy = new Message
            {
                Data = message.Data,
                Headers = message.Headers,
            };

            return copy;
        }

        public static DateTime ExecutionTime(this Message message)
        {
            return message.ToEnvelope().ExecutionTime.Value;
        }

        public static void TranslateHeaders(this OutgoingMessage messagePayload)
        {
            string headerValue;
            messagePayload.Headers.TryGetValue(LightningQueuesTransport.MaxAttemptsHeader, out headerValue);
            if (headerValue.IsNotEmpty())
            {
                messagePayload.MaxAttempts = int.Parse(headerValue);
            }
            messagePayload.Headers.TryGetValue(LightningQueuesTransport.DeliverByHeader, out headerValue);
            if (headerValue.IsNotEmpty())
            {
                messagePayload.DeliverBy = DateTime.Parse(headerValue);
            }
        }

        public static void Send(this Queue queueManager, byte[] data, IDictionary<string, string> headers, Uri address, string queueName)
        {
            var messagePayload = new OutgoingMessage
            {
                Id = MessageId.GenerateRandom(),
                Data = data,
                Headers = headers,
                SentAt = DateTime.UtcNow,
                Destination = address,
                Queue = queueName,
            };

            //TODO Maybe expose something to modify transport specific payloads?
            messagePayload.TranslateHeaders();


            queueManager.Send(messagePayload);
        }
    }

}