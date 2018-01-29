using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public interface IHasErrorHandlers
    {
        IList<IErrorHandler> ErrorHandlers { get; }
    }

    public static class ErrorHandlingConfigurationExtensions
    {
        public static void HandleErrorsWith<T>(this IHasErrorHandlers errorHandling) where T : IErrorHandler, new()
        {
            errorHandling.HandleErrorsWith(new T());
        }

        public static void HandleErrorsWith(this IHasErrorHandlers errorHandling, IErrorHandler errorHandler)
        {
            errorHandling.ErrorHandlers.Add(errorHandler);
        }


        internal static IContinuation DetermineContinuation(this IHasErrorHandlers errorHandling, Envelope envelope, Exception ex)
        {
            foreach (var handler in errorHandling.ErrorHandlers)
            {
                var continuation = handler.DetermineContinuation(envelope, ex);
                if (continuation != null) return continuation;
            }

            return null;
        }

        public static ContinuationExpression OnException<T>(this IHasErrorHandlers handlers, Func<T, bool> filter = null) where T : Exception
        {
            return new OnExceptionExpression<T>(handlers, filter);
        }

        public static ContinuationExpression OnException(this IHasErrorHandlers handlers, Type type)
        {
            if (!type.CanBeCastTo<Exception>())
            {
                throw new InvalidOperationException($"{type.FullName} is not an Exception type");
            }

            return typeof(OnExceptionExpression<>).CloseAndBuildAs<ContinuationExpression>(handlers, type);
        }

        public interface ThenContinueExpression
        {
            ContinuationExpression Then { get; }
        }

        public interface ContinuationExpression
        {
            ThenContinueExpression Retry();
            ThenContinueExpression Requeue();
            ThenContinueExpression MoveToErrorQueue();
            ThenContinueExpression RetryLater(TimeSpan delay);
            ThenContinueExpression ContinueWith(IContinuation continuation);
            ThenContinueExpression ContinueWith<TContinuation>() where TContinuation : IContinuation, new();
            ThenContinueExpression RespondWithMessage(Func<Exception, Envelope, object> messageFunc);
        }

        public class OnExceptionExpression<T> : ContinuationExpression, ThenContinueExpression where T : Exception
        {
            private readonly Lazy<ErrorHandler> _handler;

            public OnExceptionExpression(IHasErrorHandlers parent) : this(parent, e => true)
            {

            }

            public OnExceptionExpression(IHasErrorHandlers parent, Func<T, bool> filter)
            {
                _handler = new Lazy<ErrorHandler>(() =>
                {
                    var handler = new ErrorHandler();
                    handler.AddCondition(new ExceptionTypeMatch<T>(filter));
                    parent.ErrorHandlers.Add(handler);

                    return handler;
                });
            }

            public ThenContinueExpression Retry()
            {
                return ContinueWith(RetryNowContinuation.Instance);
            }

            public ThenContinueExpression Requeue()
            {
                return ContinueWith(RequeueContinuation.Instance);
            }

            public ThenContinueExpression MoveToErrorQueue()
            {
                var handler = new MoveToErrorQueueHandler<T>();
                _handler.Value.AddContinuation(handler);

                return this;
            }

            public ThenContinueExpression RetryLater(TimeSpan delay)
            {
                return ContinueWith(new ScheduledRetryContinuation(delay));
            }

            public ThenContinueExpression ContinueWith(IContinuation continuation)
            {
                _handler.Value.AddContinuation(continuation);

                return this;
            }

            public ThenContinueExpression ContinueWith<TContinuation>() where TContinuation : IContinuation, new()
            {
                return ContinueWith(new TContinuation());
            }

            ContinuationExpression ThenContinueExpression.Then => this;

            public ThenContinueExpression RespondWithMessage(Func<Exception, Envelope, object> messageFunc)
            {
                var handler = new RespondWithMessageHandler<T>(messageFunc);

                _handler.Value.AddContinuation(handler);

                return this;
            }
        }


    }
}
