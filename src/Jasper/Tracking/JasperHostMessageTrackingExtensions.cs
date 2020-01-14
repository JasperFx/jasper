using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Logging;
using Microsoft.Extensions.Hosting;

namespace Jasper.Tracking
{
    public static class JasperHostMessageTrackingExtensions
    {

        internal static MessageTrackingLogger GetTrackingLogger(this IHost host)
        {
            var logger = host.Get<IMessageLogger>() as MessageTrackingLogger;
            if (logger == null)
            {
                throw new InvalidOperationException($"The {nameof(MessageTrackingExtension)} extension is not configured for this application");
            }

            return logger;
        }

        /// <summary>
        /// Advanced usage of the 'ExecuteAndWait()' message tracking and coordination for automated testing.
        /// Use this configuration if you want to coordinate message tracking across multiple Jasper
        /// applications running in the same process
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static TrackedSessionConfiguration TrackActivity(this IHost host)
        {
            var session = new TrackedSession(host);
            return new TrackedSessionConfiguration(session);
        }

        /// <summary>
        ///     Send a message through the service bus and wait until that message
        ///     and all cascading messages have been successfully processed
        /// </summary>
        /// <param name="host"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<ITrackedSession> SendMessageAndWait<T>(this IHost host, T message,
            int timeoutInMilliseconds = 5000)
        {
            return host.ExecuteAndWait(c => c.Send(message), timeoutInMilliseconds);
        }

        /// <summary>
        /// Invoke the given message and wait until all cascading messages
        /// have completed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task<ITrackedSession> InvokeMessageAndWait(this IHost host, object message,
            int timeoutInMilliseconds = 5000)
        {
            return host.ExecuteAndWait(c => c.Invoke(message), timeoutInMilliseconds);
        }


        /// <summary>
        ///     Executes an action and waits until the execution of all messages and all cascading messages
        ///     have completed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task<ITrackedSession> ExecuteAndWait(this IHost host, Func<Task> action,
            int timeoutInMilliseconds = 5000)
        {
            return host.ExecuteAndWait(c => action(), timeoutInMilliseconds);
        }

        /// <summary>
        ///     Executes an action and waits until the execution of all messages and all cascading messages
        ///     have completed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task<ITrackedSession> ExecuteAndWait(this IHost host,
            Func<IMessageContext, Task> action,
            int timeoutInMilliseconds = 5000)
            {
                TrackedSession session = new TrackedSession(host)
                {
                    Timeout = timeoutInMilliseconds.Milliseconds(),
                    Execution = action
                };

                await session.ExecuteAndTrack();

                return session;
            }

    }
}
