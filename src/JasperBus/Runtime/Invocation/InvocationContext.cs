using System;
using System.Collections.Generic;
using Baseline;
using JasperBus.Model;

namespace JasperBus.Runtime.Invocation
{
    public class InvocationContext : IInvocationContext
    {

        private readonly IList<object> _messages = new List<object>();

        public InvocationContext(Envelope envelope, HandlerChain chain)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            Envelope = envelope;
            Chain = chain;
        }

        public Envelope Envelope { get; }
        public HandlerChain Chain { get; }


        public void EnqueueCascading(object message)
        {
            if (message == null) return;

            var enumerable = message as IEnumerable<object>;
            if (enumerable == null)
            {
                _messages.Add(message);
            }
            else
            {
                _messages.AddRange(enumerable);
            }
        }

        public IEnumerable<object> OutgoingMessages()
        {
            return _messages;
        }

        protected bool Equals(InvocationContext other)
        {
            return Equals(Envelope, other.Envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InvocationContext) obj);
        }

        public override int GetHashCode()
        {
            return (Envelope != null ? Envelope.GetHashCode() : 0);
        }
    }
}