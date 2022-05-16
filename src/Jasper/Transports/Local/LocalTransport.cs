using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.ImTools;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Transports.Local;

public class LocalTransport : ITransport
{
    private readonly Cache<string, LocalQueueSettings> _queues;

    private ImHashMap<string, ISendingAgent> _agents = ImHashMap<string, ISendingAgent>.Empty;

    public LocalTransport()
    {
        _queues = new(name => new LocalQueueSettings(name) { Root = Root });

        _queues.FillDefault(TransportConstants.Default);
        _queues.FillDefault(TransportConstants.Replies);

        _queues[TransportConstants.Durable].Mode = EndpointMode.Durable;
    }

    public Endpoint ReplyEndpoint()
    {
        return _queues[TransportConstants.Replies];
    }

    public IEnumerable<Endpoint> Endpoints()
    {
        return _queues;
    }

    public ValueTask InitializeAsync(IJasperRuntime root)
    {
        Root = root;
        foreach (var queue in _queues)
        {
            queue.Root = root;
        }

        // Nothing
        return ValueTask.CompletedTask;
    }

    internal IJasperRuntime? Root { get; private set; }

    public string Name => "Local (In Memory)";


    public ICollection<string> Protocols { get; } = new[] { TransportConstants.Local };

    void ITransport.StartSenders(IJasperRuntime root)
    {
        foreach (var queue in _queues) addQueue(root, queue);
    }

    void ITransport.StartListeners(IJasperRuntime root)
    {
        // Nothing
    }


    public Endpoint GetOrCreateEndpoint(Uri uri)
    {
        return findByUri(uri);
    }

    public Endpoint? TryGetEndpoint(Uri uri)
    {
        var queueName = QueueName(uri);
        return _queues.TryFind(queueName, out var settings) ? settings : null;
    }


    Endpoint ITransport.ListenTo(Uri uri)
    {
        return findByUri(uri);
    }

    public IEnumerable<LocalQueueSettings> AllQueues()
    {
        return _queues;
    }

    private ISendingAgent addQueue(IJasperRuntime runtime, LocalQueueSettings queue)
    {
        queue.Agent = buildAgent(queue, runtime);
        _agents = _agents.AddOrUpdate(queue.Name, buildAgent(queue, runtime));

        runtime.Endpoints.AddSendingAgent(buildAgent(queue, runtime));

        return queue.Agent;
    }

    private ISendingAgent buildAgent(LocalQueueSettings queue, IJasperRuntime root)
    {
        return queue.Mode switch
        {
            EndpointMode.BufferedInMemory => new LightweightLocalSendingAgent(queue, root.Logger, root.Pipeline,
                root.Advanced, root.MessageLogger),

            EndpointMode.Durable => new DurableLocalSendingAgent(queue, root.Pipeline, root.Advanced,
                root.Persistence, root.Logger, root.MessageLogger),

            EndpointMode.Inline => throw new NotSupportedException(),
            _ => throw new InvalidOperationException()
        };
    }

    private LocalQueueSettings findByUri(Uri uri)
    {
        var queueName = QueueName(uri);
        var settings = _queues[queueName];

        if (uri.IsDurable())
        {
            settings.Mode = EndpointMode.Durable;
        }

        return settings;
    }

    /// <summary>
    ///     Retrieves a local queue by name
    /// </summary>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public LocalQueueSettings QueueFor(string queueName)
    {
        return _queues[queueName.ToLowerInvariant()];
    }

    public static string QueueName(Uri uri)
    {
        if (uri == TransportConstants.LocalUri)
        {
            return TransportConstants.Default;
        }

        if (uri == TransportConstants.DurableLocalUri)
        {
            return TransportConstants.Durable;
        }

        if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable)
        {
            return uri.Host;
        }

        var lastSegment = uri.Segments.Skip(1).LastOrDefault();
        if (lastSegment == TransportConstants.Durable)
        {
            return TransportConstants.Default;
        }

        return lastSegment ?? TransportConstants.Default;
    }

    public static Uri AtQueue(Uri uri, string queueName)
    {
        if (queueName.IsEmpty())
        {
            return uri;
        }

        if (uri.Scheme == TransportConstants.Local && uri.Host != TransportConstants.Durable)
        {
            return new Uri("local://" + queueName);
        }

        return new Uri(uri, queueName);
    }

    internal ISendingAgent AddSenderForDestination(Uri uri, IJasperRuntime runtime)
    {
        var queueName = QueueName(uri);
        var queue = _queues[queueName];

        if (uri.IsDurable())
        {
            queue.Mode = EndpointMode.Durable;
        }

        return addQueue(runtime, queue);
    }
}
