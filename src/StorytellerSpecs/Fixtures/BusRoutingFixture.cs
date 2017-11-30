using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures
{
    public class BusRoutingFixture : BusFixture, ISubscriptionsRepository
    {
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();
        private JasperRegistry _registry;
        private MessageRoute[] _tracks;
        private JasperRuntime _runtime;

        [ExposeAsTable("The subscriptions are")]
        public void SubscriptionsAre([SelectionList("MessageTypes")]string MessageType, [SelectionList("Channels")] Uri Destination, string[] Accepts)
        {
            var messageType = messageTypeFor(MessageType);
            var subscription = new Subscription(messageType, Destination)
            {
                Accept = Accepts
            };

            _subscriptions.Add(subscription);
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

            if (_runtime == null)
            {
                _runtime = JasperRuntime.For(_registry);
            }

            var router = _runtime.Get<IMessageRouter>();

            _tracks = await router.Route(messageType);
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
            _registry.Services.For<ISubscriptionsRepository>().Use(this);
        }

        public override void TearDown()
        {
            _subscriptions.Clear();
            _runtime?.Dispose();
            _runtime = null;
        }


        Task ISubscriptionsRepository.PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            return Task.CompletedTask;
        }

        Task ISubscriptionsRepository.RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            return Task.CompletedTask;
        }

        Task<Subscription[]> ISubscriptionsRepository.GetSubscribersFor(Type messageType)
        {
            var subscriptions =  _subscriptions.Where(x => x.MessageType == messageType.ToMessageAlias()).ToArray();
            return Task.FromResult(subscriptions);
        }

        public Task<Subscription[]> GetSubscriptions()
        {
            return Task.FromResult(_subscriptions.ToArray());
        }

        public Task ReplaceSubscriptions(string serviceName, Subscription[] subscriptions)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }

    internal class FakeWriter : IMessageSerializer
    {
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

        public FakeWriter(Type messageType, string contentType)
        {
            DotNetType = messageType;
            ContentType = contentType;
        }
    }
}
