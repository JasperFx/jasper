using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Bus.Tracking
{
    public static class JasperRuntimeMessageTrackingExtensions
    {
        private static void validateMessageTrackerExists(this JasperRuntime runtime)
        {
            var history = runtime.Services.FirstOrDefault(x => x.ServiceType == typeof(MessageHistory));
            var logger = runtime.Services.Any(x =>
                x.ServiceType == typeof(IMessageLogger) && x.ImplementationType == typeof(MessageTrackingLogger));

            if (history == null || history.Lifetime != ServiceLifetime.Singleton || !logger)
            {
                throw new InvalidOperationException($"The {nameof(MessageTrackingExtension)} extension is not applied to this application");
            }
        }

        /// <summary>
        /// Send a message through the service bus and wait until that message
        /// and all cascading messages have been successfully processed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task SendMessageAndWait<T>(this JasperRuntime runtime, T message)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            return history.WatchAsync(() => runtime.Bus.Send(message));
        }



        /// <summary>
        /// Invoke a message through IServiceBus.Invoke(msg) and wait until all processing
        /// of the original message and cascading messages are complete
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task ExecuteAndWait(this JasperRuntime runtime, Func<Task> action)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            return history.WatchAsync(action);
        }

        /// <summary>
        /// Executes an action and waits until the execution and all cascading messages
        /// have completed
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task ExecuteAndWait(this JasperRuntime runtime, Action action)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            return history.Watch(action);
        }

        /// <summary>
        /// Executes a message and waits until all cascading messages are finished
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task InvokeMessageAndWait<T>(this JasperRuntime runtime, T message)
        {
            runtime.validateMessageTrackerExists();

            var history = runtime.Get<MessageHistory>();
            return history.WatchAsync(() => runtime.Bus.Invoke(message));
        }
    }
}
