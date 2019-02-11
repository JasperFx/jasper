using System;

namespace Jasper.Messaging.Transports
{
    public abstract class Endpoint<TProtocol> : IDisposable
    {
        public TransportUri Uri { get; }
        private TProtocol _protocol;

        public Endpoint(TransportUri uri, TProtocol defaultProtocol)
        {
            Uri = uri;
            _protocol = defaultProtocol;
        }

        public TProtocol Protocol
        {
            get => _protocol;
            set
            {
                if (value == null) throw new ArgumentOutOfRangeException(nameof(Protocol));
                _protocol = value;
            }
        }

        public abstract void Dispose();
    }
}