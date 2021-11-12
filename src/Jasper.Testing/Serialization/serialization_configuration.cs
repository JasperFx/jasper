using System;
using System.Threading.Tasks;
using Jasper.Runtime;
using Jasper.Serialization.New;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Serialization
{
    public class serialization_configuration
    {

        [Fact]
        public async Task by_default_every_endpoint_has_json_serializer_with_default_settings()
        {
            using var host = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                opts.Endpoints.PublishAllMessages().To("stub://one");
                opts.Endpoints.PublishAllMessages().To("stub://two");
            }).StartAsync();

            var root = host.Services.GetRequiredService<IMessagingRoot>();
            var endpoints = root.Runtime.AllEndpoints();
            foreach (var endpoint in endpoints)
            {
                endpoint.DefaultSerializer.ShouldBeOfType<NewtonsoftSerializer>()
                    .Settings.ShouldBeSameAs(root.Settings.JsonSerialization);

            }
        }

        [Fact]
        public async Task can_override_the_json_serialization_on_subscriber()
        {
            var customSettings = new JsonSerializerSettings();

            using var host = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                opts.Endpoints.PublishAllMessages().To("stub://one");

                opts.Endpoints.PublishAllMessages().To("stub://two")
                    .CustomNewtonsoftJsonSerialization(customSettings);
            }).StartAsync();

            var root = host.Services.GetRequiredService<IMessagingRoot>();
            root.Runtime.EndpointFor("stub://one".ToUri())
                .DefaultSerializer.ShouldBeOfType<NewtonsoftSerializer>()
                .Settings.ShouldBeSameAs(root.Settings.JsonSerialization);

            root.Runtime.EndpointFor("stub://two".ToUri())
                .DefaultSerializer.ShouldBeOfType<NewtonsoftSerializer>()
                .Settings.ShouldBeSameAs(customSettings);

        }

        public class FooSerializer : INewSerializer
        {
            public string ContentType { get; } = "text/foo";
            public byte[] Write(object message)
            {
                throw new NotImplementedException();
            }

            public object ReadFromData(Type messageType, byte[] data)
            {
                throw new NotImplementedException();
            }

            public object ReadFromData(byte[] data)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task can_find_other_serializer_from_parent()
        {
            var customSettings = new JsonSerializerSettings();

            using var host = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                opts.Serializers.Add(new FooSerializer());
                opts.Endpoints.PublishAllMessages().To("stub://one");

                opts.Endpoints.ListenForMessagesFrom("stub://two")
                    .CustomNewtonsoftJsonSerialization(customSettings);
            }).StartAsync();

            var root = host.Services.GetRequiredService<IMessagingRoot>();
            root.Runtime.EndpointFor("stub://one".ToUri())
                .TryFindSerializer("text/foo")
                .ShouldBeOfType<FooSerializer>();

            root.Runtime.EndpointFor("stub://two".ToUri())
                .TryFindSerializer("text/foo")
                    .ShouldBeOfType<FooSerializer>();

        }

        [Fact]
        public async Task can_override_the_default_serializer_on_sender()
        {
            var customSettings = new JsonSerializerSettings();
            var fooSerializer = new FooSerializer();

            using var host = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                opts.Endpoints.PublishAllMessages().To("stub://one")
                    .DefaultSerializer(fooSerializer);

                opts.Endpoints.ListenForMessagesFrom("stub://two")
                    .CustomNewtonsoftJsonSerialization(customSettings);
            }).StartAsync();

            var root = host.Services.GetRequiredService<IMessagingRoot>();
            root.Runtime.EndpointFor("stub://one".ToUri())
                .DefaultSerializer.ShouldBeSameAs(fooSerializer);


        }

        [Fact]
        public async Task can_override_the_json_serialization_on_listener()
        {
            var customSettings = new JsonSerializerSettings();

            using var host = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                opts.Endpoints.PublishAllMessages().To("stub://one");

                opts.Endpoints.ListenForMessagesFrom("stub://two")
                    .CustomNewtonsoftJsonSerialization(customSettings);
            }).StartAsync();

            var root = host.Services.GetRequiredService<IMessagingRoot>();
            root.Runtime.EndpointFor("stub://one".ToUri())
                .DefaultSerializer.ShouldBeOfType<NewtonsoftSerializer>()
                .Settings.ShouldBeSameAs(root.Settings.JsonSerialization);

            root.Runtime.EndpointFor("stub://two".ToUri())
                .DefaultSerializer.ShouldBeOfType<NewtonsoftSerializer>()
                .Settings.ShouldBeSameAs(customSettings);

        }

        [Fact]
        public async Task can_override_the_default_serialization_on_listener()
        {
            var fooSerializer = new FooSerializer();

            using var host = await Host.CreateDefaultBuilder().UseJasper(opts =>
            {
                opts.Endpoints.PublishAllMessages().To("stub://one");

                opts.Endpoints.ListenForMessagesFrom("stub://two")
                    .DefaultSerializer(fooSerializer);
            }).StartAsync();

            var root = host.Services.GetRequiredService<IMessagingRoot>();
            root.Runtime.EndpointFor("stub://one".ToUri())
                .DefaultSerializer.ShouldBeOfType<NewtonsoftSerializer>()
                .Settings.ShouldBeSameAs(root.Settings.JsonSerialization);

            root.Runtime.EndpointFor("stub://two".ToUri())
                .DefaultSerializer.ShouldBeSameAs(fooSerializer);

        }
    }
}
