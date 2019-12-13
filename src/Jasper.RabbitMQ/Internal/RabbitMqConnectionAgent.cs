using System;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public abstract class RabbitMqConnectionAgent : IDisposable
    {
        private readonly RabbitMqTransport _transport;
        protected readonly object _locker = new object();
        private IConnection _connection;
        internal AgentState State { get; private set; } = AgentState.Disconnected;
        internal IModel Channel { get; private set; }

        protected RabbitMqConnectionAgent(RabbitMqTransport transport)
        {
            _transport = transport;
        }

        public void Dispose()
        {
            teardownConnection();
        }

        internal void Connect()
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return;

                startNewConnection();

                State = AgentState.Connected;
            }
        }

        protected void startNewConnection()
        {
            _connection = _transport.BuildConnection();

            Channel = _connection.CreateModel();
        }

        protected void teardownConnection()
        {
            Channel?.Abort();
            Channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            Channel = null;
            _connection = null;

            State = AgentState.Disconnected;
        }

        internal void Stop()
        {
            lock (_locker)
            {
                if (State == AgentState.Disconnected) return;

                teardownConnection();
            }
        }
    }
}
