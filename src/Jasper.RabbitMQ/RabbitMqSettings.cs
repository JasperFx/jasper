using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
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

        public RabbitMqAgent(string uriString) : this (uriString.ToUri())
        {
        }

        public RabbitMqAgent(Uri uri)
        {
            if (uri.Scheme != "rabbitmq") throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");

            ConnectionFactory.Port = uri.IsDefaultPort ? 5672 : uri.Port;


            var segments = uri.Segments.Where(x => x != "/").Select(x => x.TrimEnd('/')).ToArray();
            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);

            if (segments[0] == TransportConstants.Durable)
            {
                IsDurable = uri.IsDurable();
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);


            if (ExchangeType.TryParse<ExchangeType>(segments[0], out var exchangeType))
            {
                ExchangeType = exchangeType;
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);


            if (segments.Length > 1)
            {
                ExchangeName = segments[0];
                QueueName = segments.Skip(1).Join("/");
            }
            else
            {
                QueueName = segments.Single();
            }
        }

        public bool IsDurable { get; }
        public string ExchangeName { get; }
        public ExchangeType ExchangeType { get; private set; } = ExchangeType.direct;
        public string QueueName { get; }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

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

                return agent;
            }
        }
    }
}
