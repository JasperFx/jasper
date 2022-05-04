using System;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public abstract class RabbitMqConnectionAgent : IDisposable
    {
        private readonly RabbitMqTransport _transport;
        protected readonly object _locker = new object();
        private readonly IConnection _connection;
        internal AgentState State { get; private set; } = AgentState.Disconnected;
        internal IModel Channel { get; private set; }

        protected RabbitMqConnectionAgent(IConnection connection)
        {
            _connection = connection;
        }

        public virtual void Dispose()
        {
            teardownChannel();
        }

        internal void EnsureConnected()
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return;

                startNewChannel();

                State = AgentState.Connected;
            }
        }

        protected void startNewChannel()
        {
            Channel = _connection.CreateModel();

            Channel.ModelShutdown += ChannelOnModelShutdown;
        }

        private void ChannelOnModelShutdown(object? sender, ShutdownEventArgs e)
        {
            EnsureConnected();
        }

        protected void teardownChannel()
        {
            if (Channel != null)
            {
                Channel.ModelShutdown -= ChannelOnModelShutdown;
                Channel?.Close();
                Channel?.Abort();
                Channel?.Dispose();
            }

            Channel = null;

            State = AgentState.Disconnected;
        }

    }
}
