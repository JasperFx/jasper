using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    public class ErrorHandler : IErrorHandler, IExceptionMatch
    {
        public static readonly RequeueContinuation Requeue = RequeueContinuation.Instance;

        public void AddContinuation(IContinuation continuation)
        {
            Sources.Add(new ContinuationSource(continuation));
        }

        public override string ToString()
        {
            return $"If {Conditions.Select(x => x.ToString()).Join(", «")}, then {Sources.Select(x => x.ToString()).Join(", ")}";

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
            Conditions.Add(condition);
        }

        public void AddContinuation(IContinuationSource source)
        {
            Sources.Add(source);
        }

        public IList<IContinuationSource> Sources { get; } = new List<IContinuationSource>();

        public IList<IExceptionMatch> Conditions { get; } = new List<IExceptionMatch>();


        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            return Matches(envelope, ex) ? Continuation(envelope, ex) : null;
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            if (!Conditions.Any()) return true;

            return Conditions.All(x => x.Matches(envelope, ex));
        }
    }

    public class ContinuationSource : IContinuationSource
    {
        public ContinuationSource(IContinuation continuation)
        {
            Continuation = continuation;
        }

        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            return Continuation;
        }

        public IContinuation Continuation { get; }

        public override string ToString()
        {
            return Continuation.ToString();
        }
    }

    public interface IContinuationSource
    {
        IContinuation DetermineContinuation(Envelope envelope, Exception ex);
    }
}
