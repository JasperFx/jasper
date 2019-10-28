using System;
using System.Threading.Tasks;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging.Tracking
{
    public static class JasperHostMessageTrackingExtensions
    {
        private static void validateMessageTrackerExists(this IHost host)
        {
            var history = host.Get<IContainer>().Model.For<MessageHistory>().Default;

            if (history == null || history.Lifetime != ServiceLifetime.Singleton)
                throw new InvalidOperationException(
                    $"The {nameof(MessageTrackingExtension)} extension is not applied to this application");
        }

        /// <summary>
        ///     Send a message through the service bus and wait until that message
        ///     and all cascading messages have been successfully processed
        /// </summary>
        /// <param name="host"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task SendMessageAndWait<T>(this IHost host, T message,
            int timeoutInMilliseconds = 5000, bool assertNoExceptions = false)
        {
            host.validateMessageTrackerExists();

            var history = host.Get<MessageHistory>();
            await history.WatchAsync(() => host.Get<IMessageContext>().Send(message), timeoutInMilliseconds);

            if (assertNoExceptions) history.AssertNoExceptions();
        }


        /// <summary>
        ///     Invoke a message through IServiceBus.Invoke(msg) and wait until all processing
        ///     of the original message and cascading messages are complete
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task ExecuteAndWait(this IHost runtime, Func<Task> action,
            int timeoutInMilliseconds = 5000, bool assertNoExceptions = false)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            await history.WatchAsync(action, timeoutInMilliseconds);

            if (assertNoExceptions) history.AssertNoExceptions();
        }

        /// <summary>
        ///     Executes an action and waits until the execution and all cascading messages
        ///     have completed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task ExecuteAndWait(this IHost runtime, Action action,
            bool assertNoExceptions = false)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            await history.Watch(action);

            if (assertNoExceptions) history.AssertNoExceptions();
        }

        /// <summary>
        ///     Executes an action and waits until the execution and all cascading messages
        ///     have completed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task ExecuteAndWait(this IHost runtime, Func<IMessageContext, Task> action,
            bool assertNoExceptions = false)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            var context = runtime.Get<IMessageContext>();
            await history.WatchAsync(() => action(context));

            if (assertNoExceptions) history.AssertNoExceptions();
        }

        /// <summary>
        ///     Executes a message and waits until all cascading messages are finished
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task InvokeMessageAndWait<T>(this IHost runtime, T message,
            int timeoutInMilliseconds = 5000, bool assertNoExceptions = false)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            await history.WatchAsync(() => runtime.Get<IMessageContext>().Invoke(message));

            if (assertNoExceptions) history.AssertNoExceptions();
        }
    }
}
