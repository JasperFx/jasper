using System;

namespace Jasper.Messaging.Transports
{
    public abstract class Endpoint<TProtocol>
    {
        private TProtocol _protocol;
        public string Name { get; }

        public Endpoint(string name, TProtocol defaultProtocol)
        {
            Name = name;
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
    }
}