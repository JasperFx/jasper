using System;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Invocation
{
    public class SendDirectlyTo : ISendMyself
    {
        private readonly Uri _destination;
        private readonly object _message;

        public SendDirectlyTo(Uri destination, object message)
        {
            _destination = destination;
            _message = message;
        }

        public SendDirectlyTo(string uriString, object message) : this(uriString.ToUri(), message)
        {

        }

        public Envelope CreateEnvelope(Envelope original)
        {
            var envelope = original.ForSend(_message);
            envelope.Destination = _destination;
            return envelope;
        }
    }
}
