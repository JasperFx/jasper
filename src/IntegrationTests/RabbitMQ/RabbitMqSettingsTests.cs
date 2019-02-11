using Jasper.RabbitMQ;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class RabbitMqSettingsTests
    {
        private RabbitMqSettings theSettings;

        public RabbitMqSettingsTests()
        {
            theSettings = new RabbitMqSettings();
            theSettings.Connections.Add("server1", "host=server1;queue=queue1");
            theSettings.Connections.Add("server2", "host=server2;queue=queue2");
        }

        [Theory]
        [InlineData("rabbitmq://localhost:5672/direct/one")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/two")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/two/")]
        [InlineData("rabbitmq://localhost:5672/durable/direct/four")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/three")]
        [InlineData("rabbitmq://localhost:5672/fanout/three")]
        [InlineData("rabbitmq://localhost:5672/durable/fanout/exchange1/three")]
        [InlineData("rabbitmq://localhost:5672/fanout/exchange1/three")]
        public void resolve_by_literal_uri(
            string uri)
        {
            var endpoint = theSettings.ForEndpoint(uri);
            endpoint.Uri.ShouldBe(uri.ToUri());
        }

        [Theory]
        [InlineData("rabbitmq://server1", "rabbitmq://server1:5672/direct/queue1")]
        [InlineData("rabbitmq://server2", "rabbitmq://server2:5672/direct/queue2")]
        [InlineData("rabbitmq://localhost:5672/messages3", "rabbitmq://localhost:5672/direct/messages3")]
        public void resolve_by_alias(string uri, string full)
        {
            var endpoint = theSettings.ForEndpoint(uri);
            endpoint.ToFullUri().ShouldBe(full.ToUri());
        }

    }
}
