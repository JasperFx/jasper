using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime;
using Polly;

namespace Jasper.ErrorHandling
{
    public static class JasperPollyExtensions
    {
        public const string ContextKey = "context";

        public static PolicyBuilder<IContinuation> HandledBy(this Type exceptionType)
        {
            return typeof(PolicyBuilderBuilder<>).CloseAndBuildAs<IPolicyBuilderBuilder>(exceptionType)
                .Build();
        }

        private interface IPolicyBuilderBuilder
        {
            PolicyBuilder<IContinuation> Build();
        }

        private class PolicyBuilderBuilder<T> : IPolicyBuilderBuilder where T : Exception
        {
            public PolicyBuilder<IContinuation> Build()
            {
                return Policy<IContinuation>.Handle<T>();
            }
        }

        public static void Store(this Context context, IMessageContext messageContext)
        {
            context.Add(ContextKey, messageContext);
        }

        public static IMessageContext MessageContext(this Context context)
        {
            return context[ContextKey].As<IMessageContext>();
        }

        public static IAsyncPolicy<IContinuation> Requeue(this PolicyBuilder<IContinuation> builder, int maxAttempts = 3)
        {
            return builder.FallbackAsync((result, context, token) =>
            {
                var envelope = context.MessageContext().Envelope;

                var continuation = envelope.Attempts < maxAttempts
                    ? (IContinuation) RequeueContinuation.Instance
                    : new MoveToErrorQueue(result.Exception);

                return Task.FromResult(continuation);
            }, (result, context) => Task.CompletedTask);
        }

        public static IAsyncPolicy<IContinuation> Reschedule(this PolicyBuilder<IContinuation> builder,
            params TimeSpan[] delays)
        {
            return builder.FallbackAsync((result, context, token) =>
            {
                var envelope = context.MessageContext().Envelope;

                var continuation = envelope.Attempts < delays.Length
                    ? (IContinuation) new ScheduledRetryContinuation(delays[envelope.Attempts - 1])
                    : new MoveToErrorQueue(result.Exception);

                return Task.FromResult(continuation);
            }, (result, context) => Task.CompletedTask);
        }

        public static IAsyncPolicy<IContinuation> MoveToErrorQueue(this PolicyBuilder<IContinuation> builder)
        {
            return builder.FallbackAsync((result, context, token) => Task.FromResult<IContinuation>(new MoveToErrorQueue(result.Exception)),
                (result, context) =>  Task.CompletedTask);
        }



    }
}
