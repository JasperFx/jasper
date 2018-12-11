using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Lamar;
using LamarCompiler.Util;

namespace Jasper.Messaging
{
    public class MessagingRoot : IDisposable, IMessagingRoot
    {
        private readonly HandlerGraph _handlers;
        private readonly ITransportLogger _transportLogger;
        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;

        private ImHashMap<Type, Action<Envelope>[]> _messageRules = ImHashMap<Type, Action<Envelope>[]>.Empty;

        public MessagingRoot(MessagingSerializationGraph serialization,
            JasperOptions settings,
            HandlerGraph handlers,
            IDurableMessagingFactory factory,
            ISubscriberGraph subscribers,
            IMessageLogger messageLogger,
            IContainer container,
            ITransportLogger transportLogger)
        {
            Settings = settings;
            _handlers = handlers;
            _transportLogger = transportLogger;
            Factory = factory;
            Subscribers = subscribers;
            Transports = container.QuickBuildAll<ITransport>().ToArray();


            Serialization = serialization;

            Logger = messageLogger;

            Pipeline = new HandlerPipeline(Serialization, handlers, Logger,
                container.QuickBuildAll<IMissingHandler>(),
                this);

            Workers = new WorkerQueue(Logger, Pipeline, settings);

            Router = new MessageRouter(this, handlers);

            // TODO -- ZOMG this is horrible, and I admit it.
            if (Factory is NulloDurableMessagingFactory f) f.ScheduledJobs = ScheduledJobs;
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

        public JasperOptions Settings { get; }

        public ISubscriberGraph Subscribers { get; }

        public IMessageRouter Router { get; }

        public IWorkerQueue Workers { get; }

        public IHandlerPipeline Pipeline { get; }

        public IMessageLogger Logger { get; }

        public MessagingSerializationGraph Serialization { get; }

        public IDurableMessagingFactory Factory { get; }

        public IMessageContext NewContext()
        {
            return new MessageContext(this);
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return new MessageContext(this, envelope);
        }

        public void Activate(LocalWorkerSender localWorker,
            JasperGenerationRules generation, IContainer container)
        {

            _handlers.Compile(generation, container);


            _handlers.Workers.Compile(_handlers.Chains.Select(x => x.MessageType));


            localWorker.Start(this);

            if (!Settings.DisableAllTransports)
            {
                ((SubscriberGraph) Subscribers).Start(this);
            }
        }


        public void ApplyMessageTypeSpecificRules(Envelope envelope)
        {
            if (envelope.Message == null)
                throw new ArgumentOutOfRangeException(nameof(envelope),
                    "Envelope.Message is required for this operation");

            var messageType = envelope.Message.GetType();
            if (!_messageRules.TryFind(messageType, out var rules))
            {
                rules = findMessageTypeCustomizations(messageType).ToArray();
                _messageRules = _messageRules.AddOrUpdate(messageType, rules);
            }

            foreach (var action in rules) action(envelope);
        }

        public bool ShouldBeDurable(Type messageType)
        {
            return _handlers.Workers.ShouldBeDurable(messageType);
        }

        private IEnumerable<Action<Envelope>> findMessageTypeCustomizations(Type messageType)
        {
            foreach (var att in messageType.GetAllAttributes<ModifyEnvelopeAttribute>())
                yield return e => att.Modify(e);
        }
    }
}
