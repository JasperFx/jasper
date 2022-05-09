using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Runtime;
using Microsoft.Extensions.Hosting;

namespace Jasper.Tracking;

public static class JasperHostMessageTrackingExtensions
{
    internal static JasperRuntime GetRuntime(this IHost host)
    {
        return (JasperRuntime)host.Get<IJasperRuntime>();
    }

    /// <summary>
    ///     Advanced usage of the 'ExecuteAndWait()' message tracking and coordination for automated testing.
    ///     Use this configuration if you want to coordinate message tracking across multiple Jasper
    ///     applications running in the same process
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static TrackedSessionConfiguration TrackActivity(this IHost host)
    {
        var session = new TrackedSession(host);
        return new TrackedSessionConfiguration(session);
    }

    public static TrackedSessionConfiguration TrackActivity(this IHost host, TimeSpan trackingTimeout)
    {
        var session = new TrackedSession(host);
        session.Timeout = trackingTimeout;
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
    public static Task<ITrackedSession> SendMessageAndWaitAsync<T>(this IHost host, T? message,
        int timeoutInMilliseconds = 5000)
    {
        return host.ExecuteAndWaitAsync(c => c.SendAsync(message), timeoutInMilliseconds);
    }

    /// <summary>
    ///     Invoke the given message and wait until all cascading messages
    ///     have completed
    /// </summary>
    /// <param name="runtime"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task<ITrackedSession> InvokeMessageAndWaitAsync(this IHost host, object message,
        int timeoutInMilliseconds = 5000)
    {
        return host.ExecuteAndWaitAsync(c => c.InvokeAsync(message), timeoutInMilliseconds);
    }


    /// <summary>
    ///     Executes an action and waits until the execution of all messages and all cascading messages
    ///     have completed
    /// </summary>
    /// <param name="runtime"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task<ITrackedSession> ExecuteAndWaitAsync(this IHost host, Func<Task> action,
        int timeoutInMilliseconds = 5000)
    {
        return host.ExecuteAndWaitAsync(_ => action(), timeoutInMilliseconds);
    }

    /// <summary>
    ///     Executes an action and waits until the execution of all messages and all cascading messages
    ///     have completed
    /// </summary>
    /// <param name="runtime"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task<ITrackedSession> ExecuteAndWaitAsync(this IHost host,
        Func<IExecutionContext, Task> action,
        int timeoutInMilliseconds = 5000)
    {
        var session = new TrackedSession(host)
        {
            Timeout = timeoutInMilliseconds.Milliseconds(),
            Execution = action
        };

        await session.ExecuteAndTrackAsync();

        return session;
    }
}
