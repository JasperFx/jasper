using System;
using System.Linq;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Runtime.Routing;
using Jasper.Tracking;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class topics_routing
    {
        public class TopicSendingApp : JasperOptions
        {
            public TopicSendingApp( )
            {
                Endpoints.ConfigureAzureServiceBus(asb =>
                {
                    asb.ConnectionString = end_to_end.ConnectionString;
                });

                // This directs Jasper to send all messages to
                // an Azure Service Bus topic name derived from the
                // message type
                Endpoints.PublishAllMessages()
                    .ToAzureServiceBusTopics()
                    .OutgoingTopicNameIs<NumberMessage>(x => x.Topic);
            }
        }

        public class ImplicitTopicSendingApp : JasperOptions
        {
            public ImplicitTopicSendingApp()
            {
                Endpoints.ConfigureAzureServiceBus(asb =>
                {
                    asb.ConnectionString = end_to_end.ConnectionString;
                });
            }
        }

        [Fact]
        public void route_to_topics_by_type()
        {
            using (var host = JasperHost.For<TopicSendingApp>())
            {
                var router = host.Get<IEnvelopeRouter>();

                router.RouteByType(typeof(Topic1))
                    .Routes
                    .Single()
                    .Destination
                    .ShouldBe("asb://topic/one".ToUri());

                router.RouteByType(typeof(Topic2))
                    .Routes
                    .Single()
                    .Destination
                    .ShouldBe("asb://topic/two".ToUri());

                router.RouteByType(typeof(Topic3))
                    .Routes
                    .Single()
                    .Destination
                    .ShouldBe("asb://topic/three".ToUri());
            }
        }


        [Fact]
        public void route_when_topic_is_known()
        {
            using (var host = JasperHost.For<TopicSendingApp>())
            {
                var router = host.Get<IEnvelopeRouter>();

                // Overriding the topic name here
                router.RouteOutgoingByEnvelope(new Envelope(new Topic1()){TopicName = "two"})
                    .Single()
                    .Destination
                    .ShouldBe("asb://topic/two".ToUri());
            }
        }

        [Fact]
        public void route_when_topic_is_known_implicit_registration()
        {
            using (var host = JasperHost.For<ImplicitTopicSendingApp>())
            {
                var router = host.Get<IEnvelopeRouter>();

                // Overriding the topic name here
                router.RouteOutgoingByEnvelope(new Envelope(new Topic1()){TopicName = "two"})
                    .Single()
                    .Destination
                    .ShouldBe("asb://topic/two".ToUri());
            }
        }


        [Theory]
        [InlineData("one", "asb://topic/one")]
        [InlineData("two", "asb://topic/two")]
        [InlineData("three", "asb://topic/three")]
        public void routing_with_topic_routed_by_topic_name_rule(string topicName, string uriString)
        {
            using (var host = JasperHost.For<TopicSendingApp>())
            {
                var router = host.Get<IEnvelopeRouter>();

                var message = new NumberMessage
                {
                    Topic = topicName
                };

                router.RouteOutgoingByMessage(message)
                    .Single()
                    .Destination
                    .ShouldBe(uriString.ToUri());
            }
        }


        [Theory]
        [InlineData(typeof(Topic1), "asb://topic/one")]
        [InlineData(typeof(Topic2), "asb://topic/two")]
        [InlineData(typeof(Topic3), "asb://topic/three")]
        public void route_with_topics_by_type(Type type, string uriString)
        {
            using (var host = JasperHost.For<TopicSendingApp>())
            {
                var router = host.Get<IEnvelopeRouter>();

                var message = Activator.CreateInstance(type);

                router.RouteOutgoingByMessage(message)
                    .Single()
                    .Destination
                    .ShouldBe(uriString.ToUri());
            }
        }



        [Theory]
        [InlineData("one", "asb://topic/one")]
        [InlineData("two", "asb://topic/two")]
        [InlineData("three", "asb://topic/three")]
        public void routing_with_topic_routed_by_topic_name_on_envelope(string topicName, string uriString)
        {
            using (var host = JasperHost.For<TopicSendingApp>())
            {
                var router = host.Get<IEnvelopeRouter>();

                var envelope = new Envelope(new Topic1())
                {
                    TopicName = topicName
                };

                router.RouteOutgoingByEnvelope(envelope)
                    .Single()
                    .Destination
                    .ShouldBe(uriString.ToUri());
            }
        }


    }



    public class TopicHandler
    {
        public void Handle(Topic1 one)
        {
        }

        public void Handle(Topic2 two)
        {
        }

        public void Handle(Topic3 three)
        {
        }

        public void Handle(NumberMessage message){}
    }

    [Topic("one")]
    public class Topic1{}

    [Topic("two")]
    public class Topic2{}

    [Topic("three")]
    public class Topic3{}

    public class NumberMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; }
    }
}
