using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.AzureServiceBus.Internal;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.AzureServiceBus
{


    public class AzureServiceBusEndpoint : Endpoint
    {
        public const string Queue = "queue";
        public const string Subscription = "subscription";
        public const string Topic = "topic";

        public AzureServiceBusEndpoint()
        {
        }

        public AzureServiceBusEndpoint(Uri uri) : base(uri)
        {
        }

        internal AzureServiceBusTransport Parent { get; set; }

        public string SubscriptionName { get; set; }
        public string QueueName { get; set; }
        public string TopicName { get; set; }

        public IAzureServiceBusProtocol Protocol { get; set; } = new DefaultAzureServiceBusProtocol();

        public override Uri Uri => buildUri(false);

        private Uri buildUri(bool forReply)
        {
            var list = new List<string>();

            if (QueueName.IsNotEmpty())
            {
                list.Add(Queue);
                list.Add(QueueName.ToLowerInvariant());
            }
            else
            {
                // Don't put the subscription in reply Uri
                if (!forReply && SubscriptionName.IsNotEmpty())
                {
                    list.Add(Subscription);
                    list.Add(SubscriptionName.ToLowerInvariant());
                }

                if (TopicName.IsNotEmpty())
                {
                    list.Add(Topic);
                    list.Add(TopicName.ToLowerInvariant());
                }

            }

            if (forReply && IsDurable)
            {
                list.Add(TransportConstants.Durable);
            }


            var uri = $"{AzureServiceBusTransport.ProtocolName}://{list.Join("/")}".ToUri();

            return uri;
        }

        public override Uri ReplyUri()
        {
            return buildUri(true);
        }

        public override void Parse(Uri uri)
        {
            if (uri.Scheme != AzureServiceBusTransport.ProtocolName)
            {
                throw new ArgumentOutOfRangeException($"This is not a rabbitmq Uri");
            }

            var raw = uri.Segments.Where(x => x != "/").Select(x => x.Trim('/'));
            var segments = new Queue<string>();
            segments.Enqueue(uri.Host);
            foreach (var segment in raw)
            {
                segments.Enqueue(segment);
            }


            while (segments.Any())
            {
                if (segments.Peek().EqualsIgnoreCase(Subscription))
                {
                    segments.Dequeue();
                    SubscriptionName = segments.Dequeue();
                }
                else if (segments.Peek().EqualsIgnoreCase(Queue))
                {
                    segments.Dequeue();
                    QueueName = segments.Dequeue();
                }
                else if (segments.Peek().EqualsIgnoreCase(Topic))
                {
                    segments.Dequeue();
                    TopicName = segments.Dequeue();
                }
                else if (segments.Peek().EqualsIgnoreCase(TransportConstants.Durable))
                {
                    segments.Dequeue();
                    IsDurable = true;
                }
                else
                {
                    throw new InvalidOperationException($"The Uri '{uri}' is invalid for an Azure Service Bus endpoint");
                }
            }
        }



        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            if (Parent.ConnectionString == null) throw new InvalidOperationException("There is no configured connection string for Azure Service Bus, or it is empty");

            var listener = new AzureServiceBusListener(this, Parent, root.TransportLogger, root.Cancellation);
            runtime.AddListener(listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            if (Parent.ConnectionString == null) throw new InvalidOperationException("There is no configured connection string for Azure Service Bus, or it is empty");
            return new AzureServiceBusSender(this, Parent, root.TransportLogger, root.Cancellation);
        }
    }
}
