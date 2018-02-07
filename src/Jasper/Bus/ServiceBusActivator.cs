using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueMilk.Codegen;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Jasper.Http.Transport;
using Jasper.Util;
using Microsoft.CodeAnalysis;

namespace Jasper.Bus
{
    public class ServiceBusActivator
    {
        private readonly IScheduledJobProcessor scheduledJobs;
        private readonly SerializationGraph _serialization;
        private readonly ITransport[] _transports;
        private readonly UriAliasLookup _lookups;
        private readonly IWorkerQueue _workerQueue;
        private readonly CompositeMessageLogger _logger;
        private readonly IPersistence _persistence;

        public ServiceBusActivator(IScheduledJobProcessor scheduledJobs,
            BusMessageSerializationGraph serialization, IEnumerable<ITransport> transports, UriAliasLookup lookups,
            IWorkerQueue workerQueue, CompositeMessageLogger logger, IPersistence persistence)
        {
            this.scheduledJobs = scheduledJobs;
            _serialization = serialization;
            _transports = transports.ToArray();
            _lookups = lookups;
            _workerQueue = workerQueue;
            _logger = logger;
            _persistence = persistence;
        }

        public async Task Activate(HandlerGraph handlers, CapabilityGraph capabilities, JasperRuntime runtime,
            ChannelGraph channels, LocalWorkerSender localWorker, PerfTimer timer, GenerationRules generation,
            BusSettings settings)
        {
            timer.MarkStart("ServiceBusActivator");

            handlers.Compile(generation, runtime, timer);


            var capabilityCompilation = capabilities.Compile(handlers, _serialization, channels, runtime, _transports, _lookups);





            var transports = _transports.Where(x => settings.StateFor(x.Protocol) != TransportState.Disabled)
                .ToArray();

            timer.Record("WorkersGraph.Compile", () =>
            {
                settings.Workers.Compile(handlers.Chains.Select(x => x.MessageType));
            });



            localWorker.Start(_persistence, _workerQueue);

            if (!settings.DisableAllTransports)
            {
                timer.MarkStart("ApplyLookups");

                await settings.ApplyLookups(_lookups);

                timer.MarkFinished("ApplyLookups");


                timer.Record("ChannelGraph.Start",
                    () => { channels.Start(settings, transports, _lookups, capabilities, _logger); });

                scheduledJobs.Start(_workerQueue);
            }

            runtime.Capabilities = await capabilityCompilation;
            if (runtime.Capabilities.Errors.Any() && settings.ThrowOnValidationErrors)
            {
                throw new InvalidSubscriptionException(runtime.Capabilities.Errors);
            }

            timer.MarkFinished("ServiceBusActivator");
        }

    }
}
