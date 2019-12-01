using System;

namespace Jasper.Messaging.Transports
{
    [Obsolete("Won't be necessary with the new Endpoint model'")]
    public abstract class Endpoint<TProtocol> : IDisposable
    {
        public TransportUri Uri { get; }
        private TProtocol _protocol;

        public Endpoint(TransportUri uri, TProtocol protocol)
        {
            Uri = uri;
            _protocol = protocol;
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
