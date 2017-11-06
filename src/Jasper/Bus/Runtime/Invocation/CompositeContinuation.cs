using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Bus.Runtime.Invocation
{
    public class CompositeContinuation : IContinuation, IEnumerable<IContinuation>
    {
        private readonly IList<IContinuation> _continuations = new List<IContinuation>();

        public CompositeContinuation(params IContinuation[] continuations)
        {
            _continuations.AddRange(continuations);
        }

        public async Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            foreach (var continuation in _continuations)
            {
                try
                {
                    await continuation.Execute(envelope, context, utcNow).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    context.Logger.LogException(e, envelope.Id, $"Failed trying to run continuation {continuation} as part of error handling");
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