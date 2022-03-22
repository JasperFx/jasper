using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Tracking
{
    public class TrackedSessionConfiguration
    {
        private readonly TrackedSession _session;

        public TrackedSessionConfiguration(TrackedSession session)
        {
            _session = session;
        }


        /// <summary>
        /// Override the default timeout threshold to wait for all
        /// activity to finish
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TrackedSessionConfiguration Timeout(TimeSpan timeout)
        {
            _session.Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Track activity across an additional Jasper application
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public TrackedSessionConfiguration AlsoTrack(params IHost[] hosts)
        {
            // It's actually important to ignore null here
            foreach (var host in hosts.Where(x => x != null))
            {
                _session.WatchOther(host);
            }

            return this;
        }

        /// <summary>
        /// Force the message tracking to include outgoing activity to
        /// external transports
        /// </summary>
        /// <returns></returns>
        public TrackedSessionConfiguration IncludeExternalTransports()
        {
            _session.AlwaysTrackExternalTransports = true;
            return this;
        }

        /// <summary>
        /// Do not assert or fail if exceptions where thrown during the
        /// message activity. This is useful for testing resiliency features
        /// and exception handling with message failures
        /// </summary>
        /// <returns></returns>
        public TrackedSessionConfiguration DoNotAssertOnExceptionsDetected()
        {
            _session.AssertNoExceptions = false;
            return this;
        }

        public TrackedSessionConfiguration DoNotAssertTimeout()
        {
            _session.AssertNoTimeout = false;
            return this;
        }

        public TrackedSessionConfiguration WaitForMessageToBeReceivedAt<T>(IHost host)
        {
            var condition = new WaitForMessage<T>
            {
                UniqueNodeId = host.Services.GetRequiredService<IJasperRuntime>().Settings.UniqueNodeId
            };

            _session.AddCondition(condition);

            return this;
        }

        /// <summary>
        /// Execute a user defined Lambda against an IMessageContext
        /// and wait for all activity to end
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<ITrackedSession> ExecuteAndWait(Func<IExecutionContext, Task> action)
        {
            _session.Execution = action;
            await _session.ExecuteAndTrack();
            return _session;
        }

        public async Task<ITrackedSession> ExecuteWithoutWaiting(Func<IExecutionContext, Task> action)
        {
            _session.Execution = action;
            await _session.ExecuteAndTrack();
            return _session;
        }

        /// <summary>
        /// Invoke a message inline from the current Jasper application
        /// and wait for all cascading activity to complete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<ITrackedSession> InvokeMessageAndWait(object? message)
        {
            return ExecuteAndWait(c => c.Invoke(message));
        }

        /// <summary>
        /// Send a message from the current Jasper application and wait for
        /// all cascading activity to complete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<ITrackedSession> SendMessageAndWait(object? message)
        {
            return ExecuteAndWait(c => c.SendAsync(message));
        }



    }
}
