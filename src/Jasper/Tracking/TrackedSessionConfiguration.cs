using System;
using System.Threading.Tasks;
using Jasper.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Tracking;

public class TrackedSessionConfiguration
{
    private readonly TrackedSession _session;

    public TrackedSessionConfiguration(TrackedSession session)
    {
        _session = session;
    }


    /// <summary>
    ///     Override the default timeout threshold to wait for all
    ///     activity to finish
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public TrackedSessionConfiguration Timeout(TimeSpan timeout)
    {
        _session.Timeout = timeout;
        return this;
    }

    /// <summary>
    ///     Track activity across an additional Jasper application
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public TrackedSessionConfiguration AlsoTrack(params IHost?[] hosts)
    {
        // It's actually important to ignore null here
        foreach (var host in hosts)
        {
            if (host != null)
            {
                _session.WatchOther(host);
            }
        }

        return this;
    }

    /// <summary>
    ///     Force the message tracking to include outgoing activity to
    ///     external transports
    /// </summary>
    /// <returns></returns>
    public TrackedSessionConfiguration IncludeExternalTransports()
    {
        _session.AlwaysTrackExternalTransports = true;
        return this;
    }

    /// <summary>
    ///     Do not assert or fail if exceptions where thrown during the
    ///     message activity. This is useful for testing resiliency features
    ///     and exception handling with message failures
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
            UniqueNodeId = host.Services.GetRequiredService<IJasperRuntime>().Advanced.UniqueNodeId
        };

        _session.AddCondition(condition);

        return this;
    }

    /// <summary>
    ///     Execute a user defined Lambda against an IMessageContext
    ///     and wait for all activity to end
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task<ITrackedSession> ExecuteAndWaitAsync(Func<IExecutionContext, Task> action)
    {
        _session.Execution = action;
        await _session.ExecuteAndTrackAsync();
        return _session;
    }

    /// <summary>
    ///     Execute a user defined Lambda against an IMessageContext
    ///     and wait for all activity to end
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task<ITrackedSession> ExecuteAndWaitAsync(Func<IExecutionContext, ValueTask> action)
    {
        _session.Execution = c => action(c).AsTask();
        await _session.ExecuteAndTrackAsync();
        return _session;
    }

    public async Task<ITrackedSession> ExecuteWithoutWaitingAsync(Func<IExecutionContext, Task> action)
    {
        _session.Execution = action;
        await _session.ExecuteAndTrackAsync();
        return _session;
    }

    /// <summary>
    ///     Invoke a message inline from the current Jasper application
    ///     and wait for all cascading activity to complete
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<ITrackedSession> InvokeMessageAndWaitAsync(object message)
    {
        return ExecuteAndWaitAsync(c => c.InvokeAsync(message));
    }

    /// <summary>
    ///     Send a message from the current Jasper application and wait for
    ///     all cascading activity to complete
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<ITrackedSession> SendMessageAndWaitAsync(object message, DeliveryOptions? options = null)
    {
        return ExecuteAndWaitAsync(c => c.SendAsync(message, options));
    }

    /// <summary>
    ///     Send a message from the current Jasper application and wait for
    ///     all cascading activity to complete
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public Task<ITrackedSession> SendMessageAndWaitAsync(Uri destination, object message,
        DeliveryOptions? options = null)
    {
        return ExecuteAndWaitAsync(c => c.SendAsync(destination, message, options));
    }

    /// <summary>
    ///     Publish a message from the current Jasper application and wait for
    ///     all cascading activity to complete
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<ITrackedSession> PublishMessageAndWaitAsync(object? message, DeliveryOptions? options = null)
    {
        return ExecuteAndWaitAsync(c => c.PublishAsync(message, options));
    }


    /// <summary>
    ///     Enqueue a message locally from the current Jasper application and wait for
    ///     all cascading activity to complete
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<ITrackedSession> EnqueueMessageAndWaitAsync(object? message)
    {
        return ExecuteAndWaitAsync(c => c.EnqueueAsync(message));
    }


}
