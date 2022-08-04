using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline.ImTools;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Lamar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Runtime;

public sealed partial class JasperRuntime : IJasperRuntime, IHostedService
{
    private readonly IContainer _container;

    private readonly Lazy<IEnvelopePersistence> _persistence;
    private bool _hasStopped;

    private readonly string _serviceName;
    private readonly int _uniqueNodeId;


    public JasperRuntime(JasperOptions options,
        IContainer container,
        ILogger<JasperRuntime> logger)
    {
        Advanced = options.Advanced;
        Options = options;
        Handlers = options.HandlerGraph;
        Logger = logger;

        _uniqueNodeId = options.Advanced.UniqueNodeId;
        _serviceName = options.ServiceName ?? "JasperService";

        var provider = container.GetInstance<ObjectPoolProvider>();
        ExecutionPool = provider.Create(this);

        Pipeline = new HandlerPipeline(this, this);

        _persistence = new Lazy<IEnvelopePersistence>(container.GetInstance<IEnvelopePersistence>);

        _container = container;

        Cancellation = Advanced.Cancellation;
    }

    internal IReadOnlyList<IMissingHandler> MissingHandlers()
    {
        return _container.GetAllInstances<IMissingHandler>();
    }

    public ObjectPool<ExecutionContext> ExecutionPool { get; }

    public DurabilityAgent? Durability { get; private set; }

    internal HandlerGraph Handlers { get; }

    public CancellationToken Cancellation { get; }

    private ImHashMap<Type, object?> _extensions = ImHashMap<Type, object?>.Empty;

    public T? TryFindExtension<T>() where T : class
    {
        if (_extensions.TryFind(typeof(T), out var raw)) return (T)raw;

        var extension = Options.AppliedExtensions.OfType<T>().FirstOrDefault();
        _extensions = _extensions.AddOrUpdate(typeof(T), extension);

        return extension;
    }

    public AdvancedSettings Advanced { get; }

    public ILogger Logger { get; }

    internal IScheduledJobProcessor ScheduledJobs { get; private set; } = null!;

    public JasperOptions Options { get; }

    public void ScheduleLocalExecutionInMemory(DateTimeOffset executionTime, Envelope envelope)
    {
        ScheduledJobs.Enqueue(executionTime, envelope);
    }

    public IHandlerPipeline Pipeline { get; }

    public IMessageLogger MessageLogger => this;


    public IEnvelopePersistence Persistence => _persistence.Value;
}
