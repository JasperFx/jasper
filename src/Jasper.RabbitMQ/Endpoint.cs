using System;
using Baseline;

namespace Jasper.RabbitMQ
{
    public class Endpoint
    {
        public Endpoint(string connectionString)
        {
            var parts = connectionString.ToDelimitedArray(';');
            foreach (var part in parts)
            {
                var keyValues = part.Split('=');
                if (keyValues.Length != 2) throw new ArgumentOutOfRangeException(nameof(connectionString), "The connection string is malformed");

                var key = keyValues[0];
                var value = keyValues[1];

                if (key.EqualsIgnoreCase(nameof(Host)))
                {
                    Host = value;
                }
                else if (key.EqualsIgnoreCase(nameof(Queue)))
                {
                    Queue = value;
                }
                else if (key.EqualsIgnoreCase(nameof(Port)))
                {
                    Port = int.Parse(value);
                }
                else if (key.EqualsIgnoreCase(nameof(ExchangeName)))
                {
                    ExchangeName = value;
                }
                else if (key.EqualsIgnoreCase(nameof(ExchangeType)))
                {
                    ExchangeType = (ExchangeType) Enum.Parse(typeof(ExchangeType), value, true);
                }
                else if (key.EqualsIgnoreCase(nameof(Durable)))
                {
                    Durable = bool.Parse(value);
                }
                else if (key.EqualsIgnoreCase(nameof(Topic)))
                {
                    Topic = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(connectionString), $"Unknown connection string parameter '{key}'");
                }

            }

            if (Queue.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Queue is required, but not specified");
            }

            if (Host.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Host is required, but not specified");
            }

            ServerUri = new Uri($"rabbitmq://{Host}:{Port}");
        }

        public Uri ServerUri { get; }

        public string ExchangeName { get; } = string.Empty;
        public ExchangeType ExchangeType { get; } = ExchangeType.Direct;
        public string Queue { get; }
        public string Host { get; }
        public int Port { get; } = 5672;
        public string Topic { get; } = null;

        public bool Durable { get; }

        public IEnvelopeMapper EnvelopeMapping { get; set; } = new DefaultEnvelopeMapper();
    }
}
