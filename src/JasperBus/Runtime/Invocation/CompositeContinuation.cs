using System;
using System.Collections;
using System.Collections.Generic;
using Baseline;

namespace JasperBus.Runtime.Invocation
{
    public class CompositeContinuation : IContinuation, IEnumerable<IContinuation>
    {
        private readonly IList<IContinuation> _continuations = new List<IContinuation>();

        public CompositeContinuation(params IContinuation[] continuations)
        {
            _continuations.AddRange(continuations);
        }

        public void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            foreach (var continuation in _continuations)
            {
                try
                {
                    continuation.Execute(envelope, context, utcNow);
                }
                catch (Exception e)
                {
                    context.Error(envelope.CorrelationId, "Failed trying to run continuation {0} as part of error handling".ToFormat(continuation), e);
                }
            }
        }

        public void Add(IContinuation child)
        {
            _continuations.Add(child);
        }


        public IEnumerator<IContinuation> GetEnumerator()
        {
            return _continuations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}