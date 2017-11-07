using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Conneg;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class FakeHandler<T>
    {
        public void Handle(T message)
        {
            // do nothing
        }
    }


    [Hidden]
    public class ServiceCapabilityFixture : BusFixture
    {
        private JasperRegistry _registry;

        private readonly Dictionary<Type, Type> _handlerTypes = new Dictionary<Type, Type>
        {
            {typeof(Message1), typeof(Message1Handler) },
            {typeof(Message2), typeof(Message2Handler) },
            {typeof(Message3), typeof(Message3Handler) },
            {typeof(Message4), typeof(Message4Handler) },
            {typeof(Message5), typeof(Message5Handler) },
            {typeof(Message6), typeof(Message6Handler) },
            {typeof(ErrorMessage), typeof(ErrorMessageHandler) },

        };


        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _registry.Handlers.DisableConventionalDiscovery(true);
        }

        public override void TearDown()
        {
            _registry.Settings.Alter<BusSettings>(_ =>
            {
                _.DisableAllTransports = true;
                _.ThrowOnValidationErrors = false;
            });

            ServiceCapabilities capabilities = null;
            using (var runtime = JasperRuntime.For(_registry))
            {
                capabilities = runtime.Capabilities;
            }

            Context.State.Store(capabilities);



        }

        [ExposeAsTable("The message types handled in the service are")]
        public void HandlesMessages([SelectionList("MessageTypes")] string MessageType)
        {
            var type = messageTypeFor(MessageType);

            var handlerType = _handlerTypes[type];

            _registry.Handlers.IncludeType(handlerType);
        }

        [FormatAs("Subscribes to all handled messages at {destination}")]
        public void SubscribeToAllHandledMessages(Uri destination)
        {
            _registry.Subscribe.ToAllMessages().At(destination);
        }

        [FormatAs("Subscribe to all handled messages that start with 'Message'")]
        public void SubscribeToAllMessagesStartingWithM()
        {
            _registry.Subscribe.To(t => t.Name.StartsWith("Message"));
        }

        [FormatAs("Service Name is {serviceName}")]
        public void ServiceNameIs(string serviceName)
        {
            _registry.ServiceName = serviceName;
        }

        [FormatAs("Publishes message {messageType}")]
        public void Publishes(
            [SelectionList("MessageTypes")]string messageType)
        {
            PublishesWithExtraContentTypes(messageType, new string[0]);
        }

        [FormatAs("Publishes message {MessageType} with extra content types {contentTypes}")]
        public void PublishesWithExtraContentTypes(
            [SelectionList("MessageTypes")]string MessageType,
            string[] contentTypes)
        {
            var type = messageTypeFor(MessageType);
            _registry.Publish.Message(type);

            foreach (var contentType in contentTypes)
            {
                var writer = new StubWriter(type, contentType);
                _registry.Services.For<IMessageSerializer>().Add(writer);
            }
        }

        [FormatAs("The default subscription receiver is {uri}")]
        public void DefaultSubscriptionReceiverIs([SelectionList("Channels")]string uri)
        {
            _registry.Subscribe.At(uri);
        }


        [FormatAs("Subscribes to message {messageType}")]
        public void SubscribesTo([SelectionList("MessageTypes")] string messageType)
        {
            var type = messageTypeFor(messageType);
            _registry.Subscribe.To(type);
        }

        [FormatAs("Subscribes to message {messageType} at {receiver}")]
        public void SubscribesAtLocation(
            [SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] string receiver)
        {
            var type = messageTypeFor(messageType);
            _registry.Subscribe.To(type).At(receiver);


        }

        [ExposeAsTable("The custom media readers are")]
        public void CustomReadersAre(
            [SelectionList("MessageTypes"), Header("Message Type")] string messageType,
            [Header("Content Types")]string[] contentTypes)
        {
            var type = messageTypeFor(messageType);

            foreach (var contentType in contentTypes)
            {
                var reader = new StubReader(type, contentType);
                _registry.Services.For<IMessageDeserializer>().Add(reader);
            }
        }

        [FormatAs("Additional transport schemes are {schemes}")]
        public void AdditionalTransportsAre(string[] schemes)
        {
            foreach (var scheme in schemes)
            {
                var transport = new FakeTransport(scheme);
                _registry.Services.For<ITransport>().Add(transport);
            }
        }

    }

    public class FakeTransport : ITransport
    {
        public string Protocol { get; }

        public FakeTransport(string protocol)
        {
            Protocol = protocol;
        }


        public void Dispose()
        {

        }

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Uri DefaultReplyUri()
        {
            return TransportConstants.RepliesUri;
        }

        public void StartListening(BusSettings settings)
        {

        }

        public void Describe(TextWriter writer)
        {
        }
    }
}
