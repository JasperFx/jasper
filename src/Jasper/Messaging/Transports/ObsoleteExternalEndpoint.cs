using System;

namespace Jasper.Messaging.Transports
{
    [Obsolete("Won't be necessary with the new ObsoleteExternalEndpoint model'")]
    public abstract class ObsoleteExternalEndpoint<TProtocol> : IDisposable
    {
        public TransportUri Uri { get; }
        private TProtocol _protocol;

        public ObsoleteExternalEndpoint(TransportUri uri, TProtocol protocol)
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
