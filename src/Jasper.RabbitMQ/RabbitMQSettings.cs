using System;
using System.Collections.Generic;
using Jasper.Util;

namespace Jasper.RabbitMQ
{
    public class RabbitMQSettings
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
