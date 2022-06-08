using System;
using System.Collections.Generic;
using System.Threading;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime;

public interface IJasperRuntime
{
    /// <summary>
    /// Schedule an envelope for later execution in memory
    /// </summary>
    /// <param name="executionTime"></param>
    /// <param name="envelope"></param>
    void ScheduleLocalExecutionInMemory(DateTimeOffset executionTime, Envelope envelope);

    IHandlerPipeline Pipeline { get; }
    IMessageLogger MessageLogger { get; }
    JasperOptions Options { get; }

    IEnvelopePersistence Persistence { get; }
    ILogger Logger { get; }
    AdvancedSettings Advanced { get; }
    CancellationToken Cancellation { get; }

    ISendingAgent CreateSendingAgent(Uri? replyUri, ISender sender, Endpoint endpoint);

    ISendingAgent GetOrBuildSendingAgent(Uri address, Action<Endpoint>? configureNewEndpoint = null);
    void AddListener(IListener listener, Endpoint endpoint);

    IEnumerable<IListener> ActiveListeners();

    void AddSendingAgent(ISendingAgent sendingAgent);

    Endpoint? EndpointFor(Uri uri);
    IMessageRouter RoutingFor(Type messageType);

    /// <summary>
    /// Try to find an applied extension of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? TryFindExtension<T>() where T : class;
}

// This was for testing
internal static class JasperRuntimeExtensions
{
    /// <summary>
    /// Shortcut to preview the routing for a single message
    /// </summary>
    /// <param name="runtime"></param>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Envelope[] RouteForSend(this IJasperRuntime runtime, object message, DeliveryOptions? options)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var router = runtime.RoutingFor(message.GetType());
        return router.RouteForSend(message, options);
    }
}

