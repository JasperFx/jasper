using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions.New;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class CapabilitiesFixture : BusFixture
    {
        private ServiceCapabilities _current;

        public CapabilitiesFixture()
        {
            Title = "Messaging and Subscription Fixture";
        }

        public IGrammar ForService()
        {
            return Embed<ServiceCapabilityFixture>("If a service has capabilities:")
                .After(c =>
                {
                    _current = Context.State.Retrieve<IList<ServiceCapabilities>>()
                        .LastOrDefault();
                });
        }

        [FormatAs("No capability errors were found")]
        public bool NoErrorsWereFound()
        {
            StoryTellerAssert.Fail(_current.Errors.Any(), () => $@"Found:
{_current.Errors.Select(x => $"* {x}{Environment.NewLine}")}
");
            return true;
        }

        public IGrammar TheErrorsDetectedWere()
        {
            return VerifyStringList(() => _current.Errors)
                .Titled("The detected capability errors were");
        }

        public IGrammar ThePublishedMessagesAre()
        {
            return VerifySetOf<PublishedMessage>(() => _current.Published)
                .Titled("The published messages should be")
                .MatchOn(x => x.MessageType, x => x.ContentTypes);
        }

        public IGrammar TheSubscriptionsAre()
        {
            return VerifySetOf<NewSubscription>(() => _current.Subscriptions)
                .Titled("The required subscriptions should be")
                .MatchOn(x => x.MessageType, x => x.Destination);
        }
    }

    [Hidden]
    public class MessagingGraphFixture : Fixture
    {
        private MessagingGraph _graph;
        private PublisherSubscriberMismatch _mismatch;

        public override void SetUp()
        {
            var list = Context.State.Retrieve<IList<ServiceCapabilities>>();
            _graph = new MessagingGraph(list.ToArray());
        }

        public IGrammar TheMessageTracksShouldBe()
        {
            return VerifySetOf<MessageRoute>(() => _graph.Matched)
                .Titled("The message routes should be")
                .MatchOn(x => x.MessageType, x => x.Destination, x => x.Publisher, x => x.ContentType);
        }

        public IGrammar NoSubscribersShouldBe()
        {
            return VerifySetOf<PublishedMessage>(() => _graph.NoSubscribers)
                .Titled("The messages published with no subscribers are")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.ContentTypes);
        }

        public IGrammar NoPublishersShouldBe()
        {
            return VerifySetOf<NewSubscription>(() => _graph.NoPublishers)
                .Titled("The subscriptions with no publishers are")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.Destination);
        }

        public IGrammar MismatchesAre()
        {
            return VerifySetOf<PublisherSubscriberMismatch>(() => _graph.Mismatches)
                .Titled("The detected mismatches between subscribers and publishers are")
                .MatchOn(x => x.MessageType, x => x.Publisher, x => x.Subscriber);
        }

        [FormatAs("For detected mismatch for {messageType} from {publisher} to {subscriber}")]
        public void ForMismatch([SelectionList("MessageTypes")]string messageType, string publisher, string subscriber)
        {
            _mismatch = _graph.Mismatches.FirstOrDefault(x =>
                x.MessageType == messageType && x.Publisher == publisher && x.Subscriber == subscriber);

            StoryTellerAssert.Fail(_mismatch == null, () => $@"Could not find this mismatch, the known mismatches are:
{_graph.Mismatches.Select(x => $"* {x}{Environment.NewLine}")}

");
        }


    }

    [Hidden]
    public class ServiceCapabilityFixture : BusFixture
    {
        private JasperRegistry _registry;

        public override void SetUp()
        {
            _registry = new JasperRegistry();
        }

        public override void TearDown()
        {
            _registry.Settings.Alter<BusSettings>(_ =>
            {
                _.DisableAllTransports = true;
            });

            ServiceCapabilities capabilities = null;
            using (var runtime = JasperRuntime.For(_registry))
            {
                capabilities = runtime.ServiceCapabilities;
            }

            Context.State.RetrieveOrAdd<List<ServiceCapabilities>>(() => new List<ServiceCapabilities>())
                .Add(capabilities);



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
            [SelectionList("MessageTypes")]string messageType,
            string[] contentTypes)
        {
            var type = messageTypeFor(messageType);
            _registry.Publishing.Message(type);

            foreach (var contentType in contentTypes)
            {
                var writer = new StubWriter(type, contentType);
                _registry.Services.For<IMediaWriter>().Add(writer);
            }
        }

        [FormatAs("The default subscription receiver is {uri}")]
        public void DefaultSubscriptionReceiverIs([SelectionList("Channels")]string receiver)
        {
            _registry.Subscriptions.At(receiver);
        }


        [FormatAs("Subscribes to message {messageType}")]
        public void SubscribesTo([SelectionList("MessageTypes")] string messageType)
        {
            var type = messageTypeFor(messageType);
            _registry.Subscriptions.To(type);
        }

        [FormatAs("Subscribes to message {messageType} at {receiver}")]
        public void SubscribesAtLocation(
            [SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] string receiver)
        {
            var type = messageTypeFor(messageType);
            _registry.Subscriptions.To(type).At(receiver);


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
                _registry.Services.For<IMediaReader>().Add(reader);
            }
        }

        [FormatAs("Additional transport schemes are {schemes}")]
        public void AdditionalTransportsAre(string[] schemes)
        {
            foreach (var scheme in schemes)
            {
                var transport = new StubTransport(scheme);
                _registry.Services.For<ITransport>().Add(transport);
            }
        }

    }

    public class StubReader : IMediaReader
    {
        public StubReader(Type messageType, string contentType)
        {
            MessageType = messageType.ToTypeAlias();
            DotNetType = messageType;
            ContentType = contentType;
        }

        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }
        public object ReadFromData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class StubWriter : IMediaWriter
    {
        public StubWriter(Type messageType, string contentType)
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
