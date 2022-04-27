using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using Jasper.Runtime;
using Jasper.Util;
using LamarCodeGeneration.Util;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class TopicRouterConfigurationTests
    {
        [Fact]
        public void durably()
        {
            var router = new RabbitMqTopicRouter("numbers");

            var configuration = new TopicRouterConfiguration<RabbitMqSubscriberConfiguration>(router, new JasperOptions());

            configuration.DurablyStoreAndForward();

            router.Mode.ShouldBe(EndpointMode.Durable);
        }

        [Fact]
        public void buffered_in_memory()
        {
            var router = new RabbitMqTopicRouter("numbers");
            router.Mode = EndpointMode.Inline;

            var configuration = new TopicRouterConfiguration<RabbitMqSubscriberConfiguration>(router, new JasperOptions());

            configuration.BufferedInMemory();

            router.Mode.ShouldBe(EndpointMode.BufferedInMemory);
        }

        [Fact]
        public void inline()
        {
            var router = new RabbitMqTopicRouter("numbers");
            router.Mode = EndpointMode.BufferedInMemory;

            var configuration = new TopicRouterConfiguration<RabbitMqSubscriberConfiguration>(router, new JasperOptions());

            configuration.SendInline();

            router.Mode.ShouldBe(EndpointMode.Inline);
        }

        [Fact]
        public void configure_topic_subscribers()
        {
            var options = new JasperOptions();
            options.UseRabbitMq();

            options.PublishAllMessages().ToRabbitTopics("numbers")
                .ConfigureTopicConfiguration("one", topic => { topic.SendInline(); });

            using var host = JasperHost.For(options);
            var one = host.Get<IJasperRuntime>()
                .GetOrBuildSendingAgent("rabbitmq://exchange/numbers/routing/one".ToUri())
                .Endpoint.As<RabbitMqEndpoint>();

            one.Mode.ShouldBe(EndpointMode.Inline);
        }
    }
}
