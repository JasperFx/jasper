using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Conneg;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures
{
    public class Handler<T>
    {
        public void Handle(T message)
        {
        }
    }

    public class BusRoutingFixture : BusFixture
    {
        private JasperRegistry _registry;
        private JasperRuntime _runtime;
        private MessageRoute[] _tracks;


        public Task RemoveCapabilities(string serviceName)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
        }

        [FormatAs("The application has a handler for {MessageType}")]
        public void Handles([SelectionList("MessageTypes")] string MessageType)
        {
            var messageType = messageTypeFor(MessageType);
            var handlerType = typeof(Handler<>).MakeGenericType(messageType);
            _registry.Handlers.IncludeType(handlerType);
        }


        [FormatAs("The application is configured to publish all messages locally")]
        public void PublishAllLocally()
        {
            _registry.Publish.AllMessagesLocally();
        }

        [FormatAs("The application is configured to publish the message {MessageType} locally")]
        public void PublishLocally([SelectionList("MessageTypes")] string MessageType)
        {
            var messageType = messageTypeFor(MessageType);
            _registry.Publish.Message(messageType).Locally();
        }


        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);
            _registry.Publish.MessagesMatching(type.Name, t => t == type).To(channel);

            // Just makes the test harness listen for things
            _registry.Transports.ListenForMessagesFrom(channel);
        }

        [FormatAs("Additional serializers have content types {contentTypes}")]
        public void SerializersAre(string[] contentTypes)
        {
            contentTypes.Each(contentType =>
            {
                var serializer = new FakeSerializerFactory(contentType);
                _registry.Services.For<ISerializerFactory>().Add(serializer);
            });
        }

        [ExposeAsTable("The available custom media writers are")]
        public void CustomWritersAre([SelectionList("MessageTypes")] string MessageType, string ContentType)
        {
            var messageType = messageTypeFor(MessageType);
            var writer = new FakeWriter(messageType, ContentType);
            _registry.Services.For<IMessageSerializer>().Add(writer);
        }

        [FormatAs("For message type {MessageType}")]
        public async Task ForMessage([SelectionList("MessageTypes")] string MessageType)
        {
            var messageType = messageTypeFor(MessageType);

            if (_runtime == null) _runtime = JasperRuntime.For(_registry);

            var router = _runtime.Get<IMessageRouter>();

            _tracks = await router.Route(messageType);
        }

        [FormatAs("There should be no routes")]
        public bool NoRoutesFor()
        {
            StoryTellerAssert.Fail(_tracks.Any(),
                () => { return "Found message routes:\n" + _tracks.Select(x => x.ToString()).Join("\n"); });

            return true;
        }


        public IGrammar TheRoutesShouldBe()
        {
            return VerifySetOf(() => _tracks)
                .Titled("The routes should be")
                .MatchOn(x => x.Destination, x => x.ContentType);
        }

        public override void SetUp()
        {
            _registry = new JasperRegistry();

            _registry.Handlers.DisableConventionalDiscovery();
        }

        public override void TearDown()
        {
            _runtime?.Dispose();
            _runtime = null;
        }

    }

    internal class FakeWriter : IMessageSerializer
    {
        public FakeWriter(Type messageType, string contentType)
        {
            DotNetType = messageType;
            ContentType = contentType;
        }

        public Type DotNetType { get; }
        public string ContentType { get; }

        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
