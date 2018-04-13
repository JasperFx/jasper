using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;
using Jasper.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{

    public enum ExchangeType
    {
        direct,
        topic,
        headers,
        fanout
    }


    public interface IEnvelopeMapper
    {
        Envelope From(BasicDeliverEventArgs args);
        void Apply(Envelope envelope, IBasicProperties properties);
    }

    public class DefaultEnvelopeMapper : IEnvelopeMapper
    {
        public virtual Envelope From(BasicDeliverEventArgs args)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Envelope envelope, IBasicProperties properties)
        {
            throw new NotImplementedException();
        }
    }


    public class RabbitMqAgent : IDisposable
    {
        public const string Protocol = "rabbitmq";

        public RabbitMqAgent(Uri uri)
        {
            if (uri.Host != "rabbitmq") throw new ArgumentOutOfRangeException("The protocol must be 'rabbitmq'");
        }

        public bool IsDurable { get; }
        public string ExchangeName { get; }
        public ExchangeType ExchangeType { get; private set; } = ExchangeType.direct;
        public string QueueName { get; }

        public ConnectionFactory ConnectionFactory { get; }

        public IEnvelopeMapper EnvelopeMapping { get; set; } = new DefaultEnvelopeMapper();


        // TODO -- when it initializes, it will need to create the queues
        // TODO -- when it initializes, it may need to create an exchange


        // TODO -- method to create a receiver
        // TODO -- method to create a sending agent

        // TODO -- ability to customize the creation of the connection

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }



    // TODO -- verify there are no incompatible registrations
    public class RabbitMqSettings
    {
        private readonly object _locker = new object();
        private readonly Dictionary<Uri, RabbitMqAgent> _connectionFactories = new Dictionary<Uri, RabbitMqAgent>();

        public RabbitMqAgent ForHost(string host)
        {
            return For($"rabbitmq://{host}");
        }

        public RabbitMqAgent ForHostAndPort(string host, int port)
        {
            return For($"rabbitmq://{host}:{port}");
        }

        public RabbitMqAgent For(string uriString)
        {
            return For(uriString.ToUri());
        }

        public RabbitMqAgent For(Uri uri)
        {


            // TODO -- get at the root uri, disregard queue names

            if (_connectionFactories.ContainsKey(uri)) return _connectionFactories[uri];

            lock (_locker)
            {
                if (_connectionFactories.ContainsKey(uri)) return _connectionFactories[uri];


                var agent = new RabbitMqAgent(uri);

                _connectionFactories.Add(uri, agent);
            }
        }
    }
}
