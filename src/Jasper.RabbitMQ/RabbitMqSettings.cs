using System;
using System.Collections.Concurrent;
using Jasper.Util;

// ReSharper disable InconsistentlySynchronizedField

namespace Jasper.RabbitMQ
{
    public class RabbitMqSettings
    {
        private readonly ConcurrentDictionary<Uri, RabbitMqAgent> _connectionFactories =
            new ConcurrentDictionary<Uri, RabbitMqAgent>();

        private readonly object _locker = new object();

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

                _connectionFactories[uri] = agent;

                return agent;
            }
        }
    }
}
