using System;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public abstract class RabbitMqConnectionAgent : IDisposable
    {
        private readonly IConnection _connection;
        protected readonly object Locker = new();

        protected RabbitMqConnectionAgent(IConnection connection)
        {
            _connection = connection;
        }

        internal AgentState State { get; private set; } = AgentState.Disconnected;

        private IModel? _channel;

        internal IModel Channel
        {
            get
            {
                if (_channel == null)
                {
                    EnsureConnected();
                }

                return _channel!;
            }
        }

        public virtual void Dispose()
        {
            teardownChannel();
        }

        internal void EnsureConnected()
        {
            lock (Locker)
            {
                if (State == AgentState.Connected)
                {
                    return;
                }

                startNewChannel();

                State = AgentState.Connected;
            }
        }

        protected void startNewChannel()
        {
            _channel = _connection.CreateModel();

            _channel.ModelShutdown += ChannelOnModelShutdown;
        }

        private void ChannelOnModelShutdown(object? sender, ShutdownEventArgs e)
        {
            EnsureConnected();
        }

        protected void teardownChannel()
        {
            if (_channel != null)
            {
                _channel.ModelShutdown -= ChannelOnModelShutdown;
                _channel.Close();
                _channel.Abort();
                _channel.Dispose();
            }

            _channel = null;

            State = AgentState.Disconnected;
        }
    }
}
