using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Attributes;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class topics_routing : IAsyncLifetime
    {
        private IHost _host;

        public async Task InitializeAsync()
        {
            _host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.UseRabbitMq().AutoProvision()
                        .DeclareExchange("numbers", ExchangeType.Topic);

                    // This directs Jasper to send all messages
                    // to the "numbers" exchange in Rabbit MQ with
                    // a routing key derived from the message type
                    opts.PublishAllMessages()
                        .ToRabbitTopics("numbers")
                        .OutgoingTopicNameIs<NumberMessage>(x => x.Topic);

                }).StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        [Theory]
        [InlineData("one", "rabbitmq://exchange/numbers/routing/one")]
        [InlineData("two", "rabbitmq://exchange/numbers/routing/two")]
        [InlineData("three", "rabbitmq://exchange/numbers/routing/three")]
        public void routing_with_topic_routed_by_topic_name_rule(string topicName, string uriString)
        {
            var router = _host.Get<IEnvelopeRouter>();

            var message = new NumberMessage
            {
                Topic = topicName
            };

            router.RouteOutgoingByMessage(message)
                .Single()
                .Destination
                .ShouldBe(uriString.ToUri());
        }

        [Theory]
        [InlineData(typeof(Topic1), "rabbitmq://exchange/numbers/routing/one")]
        [InlineData(typeof(Topic2), "rabbitmq://exchange/numbers/routing/two")]
        [InlineData(typeof(Topic3), "rabbitmq://exchange/numbers/routing/three")]
        public void route_with_topics_by_type(Type type, string uriString)
        {
            var router = _host.Get<IEnvelopeRouter>();

            var message = Activator.CreateInstance(type);

            router.RouteOutgoingByMessage(message)
                .Single()
                .Destination
                .ShouldBe(uriString.ToUri());
        }


        [Theory]
        [InlineData("one", "rabbitmq://exchange/numbers/routing/one")]
        [InlineData("two", "rabbitmq://exchange/numbers/routing/two")]
        [InlineData("three", "rabbitmq://exchange/numbers/routing/three")]
        public void routing_with_topic_routed_by_topic_name_on_envelope(string topicName, string uriString)
        {
            var router = _host.Get<IEnvelopeRouter>();

            var envelope = new Envelope(new Topic1())
            {
                TopicName = topicName
            };

            router.RouteOutgoingByEnvelope(envelope)
                .Single()
                .Destination
                .ShouldBe(uriString.ToUri());
        }

        [Fact]
        public void route_to_topics_by_type()
        {
            var router = _host.Get<IEnvelopeRouter>().As<JasperRuntime>();

            router.RouteByType(typeof(Topic1))
                .Routes
                .Single()
                .Destination
                .ShouldBe("rabbitmq://exchange/numbers/routing/one".ToUri());

            router.RouteByType(typeof(Topic2))
                .Routes
                .Single()
                .Destination
                .ShouldBe("rabbitmq://exchange/numbers/routing/two".ToUri());

            router.RouteByType(typeof(Topic3))
                .Routes
                .Single()
                .Destination
                .ShouldBe("rabbitmq://exchange/numbers/routing/three".ToUri());
        }

        [Fact]
        public void route_when_topic_is_known()
        {
            var router = _host.Get<IEnvelopeRouter>();

            // Overriding the topic name here
            router.RouteOutgoingByEnvelope(new Envelope(new Topic1()) { TopicName = "two" })
                .Single()
                .Destination
                .ShouldBe("rabbitmq://exchange/numbers/routing/two".ToUri());
        }

        [Fact]
        public void throw_descriptive_description_with_no_topic_routers()
        {
            using (var host = JasperHost.Basic())
            {
                var router = host.Get<IEnvelopeRouter>();

                Should.Throw<InvalidOperationException>(() => router.RouteToTopic("one", new Envelope(new Topic1())));
            }
        }

        internal static async Task RabbitMqTopicSendingApp()
        {
            #region sample_RabbitMqTopicSendingApp

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.UseRabbitMq()
                        .AutoProvision()
                        .DeclareExchange("numbers", ExchangeType.Topic);


                    // This directs Jasper to send all messages
                    // to the "numbers" exchange in Rabbit MQ with
                    // a routing key derived from the message type
                    opts.PublishAllMessages()
                        .ToRabbitTopics("numbers")
                        .OutgoingTopicNameIs<NumberMessage>(x => x.Topic);
                }).StartAsync();

            #endregion
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

        public void Handle(NumberMessage message)
        {
        }
    }

    [Topic("one")]
    public class Topic1
    {
    }

    [Topic("two")]
    public class Topic2
    {
    }

    [Topic("three")]
    public class Topic3
    {
    }

    public class NumberMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; }
    }
}
