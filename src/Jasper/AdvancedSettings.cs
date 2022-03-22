using System;
using System.Reflection;
using System.Threading;
using Baseline.Dates;
using Jasper.Util;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Newtonsoft.Json;

namespace Jasper
{
    public enum StorageProvisioning
    {
        /// <summary>
        /// Production mode and the default. Do not rebuild or clear envelope
        /// storage upon system start
        /// </summary>
        None,

        /// <summary>
        /// Drop and rebuild the message storage on application startup. Only suitable
        /// for development or testing
        /// </summary>
        Rebuild,

        /// <summary>
        /// Clear all the persisted message storage on application startup. Only suitable
        /// for development or testing
        /// </summary>
        Clear
    }

    public class AdvancedSettings
    {


        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private string? _serviceName;


        public AdvancedSettings(Assembly applicationAssembly)
        {
            var name = applicationAssembly?.GetName().Name ?? "JasperApplication";
            CodeGeneration = new GenerationRules("Internal.Generated");
            CodeGeneration.Sources.Add(new NowTimeVariableSource());

            CodeGeneration.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            CodeGeneration.Assemblies.Add(applicationAssembly);
        }

        /// <summary>
        ///     Configure or extend the Lamar code generation
        /// </summary>
        public GenerationRules CodeGeneration { get; }

        internal void Cancel()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        /// Should the message durability agent be enabled during execution.
        /// The default is true.
        /// </summary>
        public bool DurabilityAgentEnabled { get; set; } = true;

        public StorageProvisioning StorageProvisioning { get; set; } = StorageProvisioning.None;

        /// <summary>
        /// Default is false. Turn this on to see when every polling DurablityAgent
        /// action executes. Warning, it's a LOT of noise
        /// </summary>
        public bool VerboseDurabilityAgentLogging { get; set; } = false;



        /// <summary>
        /// Governs the page size for how many persisted incoming or outgoing messages
        /// will be loaded at one time for attempted retries
        /// </summary>
        public int RecoveryBatchSize { get; set; } = 100;

        /// <summary>
        /// How frequently Jasper will attempt to reassign incoming or outgoing
        /// persisted methods from nodes that are detected to be offline
        /// </summary>
        public TimeSpan NodeReassignmentPollingTime { get; set; } = 1.Minutes();

        /// <summary>
        /// When should the first execution of the node reassignment job
        /// execute after application startup.
        /// </summary>
        public TimeSpan FirstNodeReassignmentExecution { get; set; } = 0.Seconds();

        /// <summary>
        ///     Default is true. Should Jasper throw an exception on start up if any validation errors
        ///     are detected
        /// </summary>
        public bool ThrowOnValidationErrors { get; set; } = true;


        /// <summary>
        ///     Interval between collecting persisted and queued message metrics
        /// </summary>
        public TimeSpan MetricsCollectionSamplingInterval { get; set; } = 5.Seconds();

        /// <summary>
        /// How long to wait before the first execution of polling
        /// for ready, persisted scheduled messages
        /// </summary>
        public TimeSpan ScheduledJobFirstExecution { get; set; } = 0.Seconds();

        /// <summary>
        /// Polling interval for executing scheduled messages
        /// </summary>
        public TimeSpan ScheduledJobPollingTime { get; set; } = 5.Seconds();

        /// <summary>
        /// Newtonsoft.Json serialization settings for messages sent or received
        /// </summary>
        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public int UniqueNodeId { get; } = Guid.NewGuid().ToString().GetDeterministicHashCode();

        public CancellationToken Cancellation => _cancellation.Token;

        /// <summary>
        ///     Get or set the logical Jasper service name. By default, this is
        ///     derived from the name of a custom JasperOptions
        /// </summary>

        public string? ServiceName
        {
            get => _serviceName;
            set
            {
                _serviceName = value ?? Assembly.GetEntryAssembly().GetName().Name;
                OpenTelemetryProcessSpanName = $"{_serviceName} process";
                OpenTelemetrySendSpanName = $"{_serviceName} send";
                OpenTelemetryReceiveSpanName = $"{_serviceName} receive";
            }

        }

        public string? OpenTelemetryProcessSpanName { get; private set; }
        public string? OpenTelemetrySendSpanName { get; private set; }
        public string? OpenTelemetryReceiveSpanName { get; private set; }

        /// <summary>
        /// This should probably *only* be used in development or testing
        /// to latch all outgoing message sending
        /// </summary>
        public bool StubAllOutgoingExternalSenders { get; set; }
    }
}
