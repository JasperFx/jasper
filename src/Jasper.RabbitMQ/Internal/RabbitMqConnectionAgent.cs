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

        public virtual void Dispose()
        {
            teardownConnection();
        }

        internal void EnsureConnected()
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

            Channel.ModelShutdown += ChannelOnModelShutdown;
        }

        private void ChannelOnModelShutdown(object? sender, ShutdownEventArgs e)
        {
            EnsureConnected();
        }

        protected void teardownConnection()
        {
            Channel.ModelShutdown -= ChannelOnModelShutdown;
            Channel?.Close();
            Channel?.Abort();
            Channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            Channel = null;
            _connection = null;

            State = AgentState.Disconnected;
        }

    }
}
