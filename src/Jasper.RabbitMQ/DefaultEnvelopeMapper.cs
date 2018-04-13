using System;
using Jasper.Messaging.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public class DefaultEnvelopeMapper : IEnvelopeMapper
    {
        public virtual Envelope From(BasicDeliverEventArgs args)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Envelope envelope, IBasicProperties properties)
        {
            throw new NotImplementedException();
        }
    }
}