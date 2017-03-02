using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
{
    public class ErrorHandler : IErrorHandler, IExceptionMatch
    {
        public static readonly RequeueContinuation Requeue = RequeueContinuation.Instance;

        private readonly IList<IExceptionMatch> _conditions = new List<IExceptionMatch>();


        public void AddContinuation(IContinuation continuation)
        {
            Sources.Add(new ContinuationSource(continuation));
        }

        public IContinuation Continuation(Envelope envelope, Exception ex)
        {
            var count = Sources.Count;
            switch (count)
            {
                case 0:
                    return Requeue;

                case 1:
                    return Sources.Single().DetermineContinuation(envelope, ex);

                default:
                    return new CompositeContinuation(Sources.Select(x => x.DetermineContinuation(envelope, ex)).ToArray());
            }
        }

        public void AddCondition(IExceptionMatch condition)
        {
            _conditions.Add(condition);
        }

        public void AddContinuation(IContinuationSource source)
        {
            Sources.Add(source);
        }

        public IList<IContinuationSource> Sources { get; } = new List<IContinuationSource>();

        public IEnumerable<IExceptionMatch> Conditions => _conditions;

        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            return Matches(envelope, ex) ? Continuation(envelope, ex) : null;
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            if (!_conditions.Any()) return true;

            return _conditions.All(x => x.Matches(envelope, ex));
        }
    }

    public class ContinuationSource : IContinuationSource
    {
        private readonly IContinuation _continuation;

        public ContinuationSource(IContinuation continuation)
        {
            _continuation = continuation;
        }

        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            return _continuation;
        }
    }

    public interface IContinuationSource
    {
        IContinuation DetermineContinuation(Envelope envelope, Exception ex);
    }
}