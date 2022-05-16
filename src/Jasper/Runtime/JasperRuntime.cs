using System;
using System.Linq;
using System.Threading;
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
        var pool = provider.Create(this);

        // TODO -- might make NoHandlerContinuation lazy!
        Pipeline = new HandlerPipeline(Handlers, this,
            new NoHandlerContinuation(container.GetAllInstances<IMissingHandler>().ToArray(), this),
            this, pool);

        _persistence = new Lazy<IEnvelopePersistence>(container.GetInstance<IEnvelopePersistence>);

        _container = container;

        Cancellation = Advanced.Cancellation;
    }

    public DurabilityAgent? Durability { get; private set; }

    internal HandlerGraph Handlers { get; }

    public IJasperEndpoints Endpoints => this;

    public CancellationToken Cancellation { get; }

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
