using System;
using System.Collections.Generic;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqMessageSpecificTopicListeningAgent : IListeningAgent
    {
        private readonly IList<IListeningAgent> _inner = new List<IListeningAgent>();
        private ListeningStatus _status = ListeningStatus.Accepting;


        public RabbitMqMessageSpecificTopicListeningAgent(RabbitMqEndpoint endpoint, HandlerGraph handlers,
            TransportUri transportUri, ITransportLogger logger, JasperOptions settings)
        {
            Address = endpoint.Uri.ToUri();

            var endpoints = endpoint.SpreadForMessageSpecificTopics(handlers.ValidMessageTypeNames());
            foreach (var topicEndpoint in endpoints)
            {
                topicEndpoint.Connect();
                var agent = topicEndpoint.CreateListeningAgent(transportUri.ToUri(), settings, logger);
                _inner.Add(agent);
            }
        }

        public void Dispose()
        {
            foreach (var agent in _inner)
            {
                agent.Dispose();
            }
        }

        public Uri Address { get; }


        public ListeningStatus Status
        {
            get => _status;
            set
            {
                _status = value;

                foreach (var agent in _inner)
                {
                    agent.Status = value;
                }
            }
        }

        public void Start(IReceiverCallback callback)
        {
            foreach (var agent in _inner)
            {
                agent.Start(callback);
            }
        }


    }
}
