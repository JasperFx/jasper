using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Lamar;
using LamarCodeGeneration;

namespace Jasper.Messaging
{
    public class MessagingRoot : IDisposable, IMessagingRoot
    {
        private readonly ITransportLogger _transportLogger;
        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;

        private ImHashMap<Type, Action<Envelope>[]> _messageRules = ImHashMap<Type, Action<Envelope>[]>.Empty;

        private readonly Lazy<IEnvelopePersistence> _persistence;

        public MessagingRoot(MessagingSerializationGraph serialization,
            JasperOptions options,
            HandlerGraph handlers,
            ISubscriberGraph subscribers,
            IMessageLogger messageLogger,
            IContainer container,
            ITransportLogger transportLogger
            )
        {
            Options = options;
            Handlers = handlers;
            _transportLogger = transportLogger;
            Subscribers = subscribers;
            Transports = container.QuickBuildAll<ITransport>().ToArray();


            Serialization = serialization;

            Logger = messageLogger;

            Pipeline = new HandlerPipeline(Serialization, handlers, Logger,
                container.QuickBuildAll<IMissingHandler>(),
                this);

            Workers = new WorkerQueue(Logger, Pipeline, options);

            Router = new MessageRouter(this, handlers);

            _persistence = new Lazy<IEnvelopePersistence>(() => container.GetInstance<IEnvelopePersistence>());
        }

        public void Dispose()
        {
            ScheduledJobs.Dispose();
        }

        public ListeningStatus ListeningStatus
        {
            get => _listeningStatus;
            set
            {
                _transportLogger.ListeningStatusChange(value);
                _listeningStatus = value;


                foreach (var transport in Transports) transport.ListeningStatus = value;
            }
        }

        public ITransport[] Transports { get; }

        public IScheduledJobProcessor ScheduledJobs => Workers.ScheduledJobs;

        public JasperOptions Options { get; }

        public ISubscriberGraph Subscribers { get; }

        public IMessageRouter Router { get; }

        public IWorkerQueue Workers { get; }

        public IHandlerPipeline Pipeline { get; }

        public IMessageLogger Logger { get; }

        public MessagingSerializationGraph Serialization { get; }

        public IEnvelopePersistence Persistence => _persistence.Value;

        public IMessageContext NewContext()
        {
            return new MessageContext(this);
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return new MessageContext(this, envelope);
        }

        public async Task Activate(GenerationRules generation, IContainer container)
        {
            Handlers.Compile(generation, container);


            Handlers.Workers.Compile(Handlers.Chains.Select(x => x.MessageType));



            if (generation.TypeLoadMode == TypeLoadMode.LoadFromPreBuiltAssembly)
            {
                await container.GetInstance<DynamicCodeBuilder>().LoadPrebuiltTypes();
            }

            ((SubscriberGraph) Subscribers).Start(this);
        }

        public HandlerGraph Handlers { get; }


        public void ApplyMessageTypeSpecificRules(Envelope envelope)
        {
            if (envelope.Message == null) return;

            var messageType = envelope.Message.GetType();
            if (!_messageRules.TryFind(messageType, out var rules))
            {
                rules = findMessageTypeCustomizations(messageType).ToArray();
                _messageRules = _messageRules.AddOrUpdate(messageType, rules);
            }

            foreach (var action in rules) action(envelope);
        }

        [Obsolete("Get rid of this")]
        public bool ShouldBeDurable(Type messageType)
        {
            return Handlers.Workers.ShouldBeDurable(messageType);
        }

        public ISendingAgent BuildDurableSendingAgent(Uri destination, ISender sender)
        {
            return new DurableSendingAgent(destination, sender, _transportLogger, Options, Persistence);
        }

        public ISendingAgent BuildDurableLoopbackAgent(Uri destination)
        {
            return new DurableLoopbackSendingAgent(destination, Workers, Persistence, Serialization, _transportLogger, Options);
        }

        private IEnumerable<Action<Envelope>> findMessageTypeCustomizations(Type messageType)
        {
            foreach (var att in messageType.GetAllAttributes<ModifyEnvelopeAttribute>())
                yield return e => att.Modify(e);
        }
    }
}
