using System;
using System.Linq;
using System.Threading;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Lamar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Runtime;

public partial class JasperRuntime : IJasperRuntime, IHostedService
{
    private readonly IContainer _container;

    private readonly Lazy<IEnvelopePersistence> _persistence;
    private bool _hasStopped;


    public JasperRuntime(JasperOptions options,
        IMessageLogger messageLogger,
        IContainer container,
        ILogger<JasperRuntime> logger)
    {
        Advanced = options.Advanced;
        Options = options;
        Handlers = options.HandlerGraph;
        Logger = logger;

        MessageLogger = messageLogger;

        var provider = container.GetInstance<ObjectPoolProvider>();
        var pool = provider.Create(this);

        // TODO -- might make NoHandlerContinuation lazy!
        Pipeline = new HandlerPipeline(Handlers, MessageLogger,
            new NoHandlerContinuation(container.GetAllInstances<IMissingHandler>().ToArray(), this),
            this, pool);

        Runtime = new TransportRuntime(this);

        _persistence = new Lazy<IEnvelopePersistence>(container.GetInstance<IEnvelopePersistence>);

        Router = new EnvelopeRouter(this);

        Acknowledgements = new AcknowledgementSender(Router, this);

        _container = container;

        Cancellation = Advanced.Cancellation;
    }


    public DurabilityAgent Durability { get; private set; }

    internal HandlerGraph Handlers { get; }

    public IAcknowledgementSender Acknowledgements { get; }


    public ITransportRuntime Runtime { get; }
    public CancellationToken Cancellation { get; }

    public AdvancedSettings Advanced { get; }

    public ILogger Logger { get; }

    public IScheduledJobProcessor ScheduledJobs { get; set; }

    public JasperOptions Options { get; }

    public IEnvelopeRouter Router { get; }

    public IHandlerPipeline Pipeline { get; }

    public IMessageLogger MessageLogger { get; }


    public IEnvelopePersistence Persistence => _persistence.Value;
}
