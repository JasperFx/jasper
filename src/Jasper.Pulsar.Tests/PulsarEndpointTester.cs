using System;
using AutoFixture.Xunit2;
using Shouldly;
using Xunit;

namespace Jasper.Pulsar.Tests
{
    public class PulsarEndpointTester
    {
        [Fact]
        public void parse_non_durable_uri()
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1"));

            endpoint.IsDurable.ShouldBeFalse();
            endpoint.Topic.TopicName.ShouldBe("key1");
        }

        [Theory, AutoData]
        public void persistent_pulsar_topic_parts_match(string tenant, string @namespace, string topic)
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.Persistent}://{tenant}/{@namespace}/{topic}"));

            endpoint.Topic.Persistence.ShouldBe(PulsarPersistence.Persistent);
            endpoint.Topic.Tenant.ShouldBe(tenant);
            endpoint.Topic.Namespace.ShouldBe(@namespace);
            endpoint.Topic.TopicName.ShouldBe(topic);
        }

        [Theory, AutoData]
        public void non_persistent_pulsar_topic_parts_match(string tenant, string @namespace, string topic)
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.NonPersistent}://{tenant}/{@namespace}/{topic}"));

            endpoint.Topic.Persistence.ShouldBe(PulsarPersistence.NonPersistent);
            endpoint.Topic.Tenant.ShouldBe(tenant);
            endpoint.Topic.Namespace.ShouldBe(@namespace);
            endpoint.Topic.TopicName.ShouldBe(topic);
        }

        [Fact]
        public void parse_non_durable_persistent_uri()
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1"));

            endpoint.IsDurable.ShouldBeFalse();
            endpoint.Topic.Persistence.ShouldBe(PulsarPersistence.Persistent);
        }

        [Fact]
        public void parse_non_durable_non_persistent_uri()
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.NonPersistent}://tenant/jasper/key1"));

            endpoint.IsDurable.ShouldBeFalse();
            endpoint.Topic.Persistence.ShouldBe(PulsarPersistence.NonPersistent);
        }

        [Fact]
        public void parse_durable_persistent_uri()
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1/durable"));

            endpoint.IsDurable.ShouldBeTrue();
            endpoint.Topic.Persistence.ShouldBe(PulsarPersistence.Persistent);
        }

        [Fact]
        public void parse_durable_non_persistent_uri()
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.NonPersistent}://tenant/jasper/key1/durable"));

            endpoint.IsDurable.ShouldBeTrue();
            endpoint.Topic.Persistence.ShouldBe(PulsarPersistence.NonPersistent);
        }

        [Fact]
        public void parse_durable_uri()
        {
            var endpoint = new PulsarEndpoint();
            endpoint.Parse(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1/durable"));

            endpoint.IsDurable.ShouldBeTrue();
            endpoint.Topic.TopicName.ShouldBe("key1");
        }

        [Fact]
        public void build_uri_for_subscription_and_topic()
        {
            new PulsarEndpoint
                {
                    Topic = $"{PulsarPersistence.Persistent}://tenant/jasper/key1"
            }
                .Uri.ShouldBe(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_non_durable()
        {
            new PulsarEndpoint
                {
                    Topic = $"{PulsarPersistence.Persistent}://tenant/jasper/key1"
                }
                .ReplyUri().ShouldBe(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1"));
        }

        [Fact]
        public void generate_reply_uri_for_durable()
        {
            new PulsarEndpoint
            {
                Topic = $"{PulsarPersistence.Persistent}://tenant/jasper/key1",
                IsDurable = true
            }.ReplyUri().ShouldBe(new Uri($"{PulsarPersistence.Persistent}://tenant/jasper/key1/durable"));
        }

    }
}
