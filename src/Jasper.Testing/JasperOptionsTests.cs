using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Sending;
using Jasper.Util;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using TestingSupport;
using TestingSupport.Fakes;
using Xunit;

namespace Jasper.Testing
{
    public class JasperOptionsTests
    {
        private readonly JasperOptions theSettings = new JasperOptions();

        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        [Fact]
        public void unique_node_id_is_really_unique()
        {
            var options1 = new AdvancedSettings(null);
            var options2 = new AdvancedSettings(null);
            var options3 = new AdvancedSettings(null);
            var options4 = new AdvancedSettings(null);
            var options5 = new AdvancedSettings(null);
            var options6 = new AdvancedSettings(null);

            options1.UniqueNodeId.ShouldNotBe(options2.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options3.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options2.UniqueNodeId.ShouldNotBe(options3.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options3.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options3.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options3.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options4.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options4.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options5.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);
        }

        [Fact]
        public void sets_up_the_container_with_services()
        {
            var registry = new JasperOptions();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Services.For<IFoo>().Use<Foo>();
            registry.Services.AddTransient<IFakeStore, FakeStore>();

            using (var runtime = JasperHost.For(registry))
            {
                runtime.Get<IContainer>().DefaultRegistrationIs<IFoo, Foo>();
            }
        }

        [Fact]
        public void stub_out_external_setting_via_IEndpoints()
        {
            var options = new JasperOptions();
            options.Advanced.StubAllOutgoingExternalSenders.ShouldBeFalse();

            options.StubAllExternallyOutgoingEndpoints();

            options.Advanced.StubAllOutgoingExternalSenders.ShouldBeTrue();
        }

        [Fact]
        public void use_the_calling_assembly_name_if_it_is_a_basic_registry()
        {
            new JasperOptions().ServiceName.ShouldBe("Jasper.Testing");
        }

                [Fact]
        public void add_transport()
        {
            var transport = Substitute.For<ITransport>();
            transport.Protocols.Returns(new []{"fake"});

            var collection = new JasperOptions() {transport};

            collection.ShouldContain(transport);


        }

        [Fact]
        public void try_to_get_endpoint_from_invalid_transport()
        {
            var collection = new JasperOptions();
            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                collection.TryGetEndpoint("wrong://server".ToUri());
            });
        }

        [Fact]
        public void local_is_registered_by_default()
        {
            new JasperOptions()
                .OfType<LocalTransport>()
                .Count().ShouldBe(1);
        }

        [Fact]
        public void retrieve_transport_by_scheme()
        {
            new JasperOptions()
                .TransportForScheme("local")
                .ShouldBeOfType<LocalTransport>();
        }

        [Fact]
        public void retrieve_transport_by_type()
        {
            new JasperOptions()
                .Get<LocalTransport>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void all_endpoints()
        {
            var collection = new JasperOptions();
            collection.ListenForMessagesFrom("stub://one");
            collection.PublishAllMessages().To("stub://two");

            // 2 default local queues + the 2 added here
            collection.AllEndpoints()
                .Length.ShouldBe(5);
        }

        [Fact]
        public void publish_mechanism_with_multiple_subscribers()
        {
            var collection = new JasperOptions();
            collection.Publish(x =>
            {
                x.MessagesFromNamespace("One");
                x.MessagesFromNamespace("Two");

                x.To("stub://3333");
                x.To("stub://4444");
            });

            var endpoint3333 = collection.TryGetEndpoint("stub://3333".ToUri());
            var endpoint4444 = collection.TryGetEndpoint("stub://4444".ToUri());

            endpoint3333.Subscriptions[0]
                .ShouldBe(new Subscription{Scope = RoutingScope.Namespace, Match = "One"});

            endpoint3333.Subscriptions[1]
                .ShouldBe(new Subscription{Scope = RoutingScope.Namespace, Match = "Two"});

            endpoint4444.Subscriptions[0]
                .ShouldBe(new Subscription{Scope = RoutingScope.Namespace, Match = "One"});

            endpoint4444.Subscriptions[1]
                .ShouldBe(new Subscription{Scope = RoutingScope.Namespace, Match = "Two"});

        }

        [Fact]
        public void create_transport_type_if_missing()
        {
            var collection = new JasperOptions();
            var transport = collection.Get<FakeTransport>();

            collection.Get<FakeTransport>()
                .ShouldBeSameAs(transport);
        }

        public class FakeTransport : ITransport
        {
            public string Name { get; } = "Fake";

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public ICollection<string> Protocols { get; } = new []{"fake"};
            public ISendingAgent BuildSendingAgent(Uri uri, IJasperRuntime root, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }


            public Endpoint ReplyEndpoint()
            {
                throw new NotImplementedException();
            }

            public Endpoint ListenTo(Uri? uri)
            {
                throw new NotImplementedException();
            }

            public void StartSenders(IJasperRuntime root, ITransportRuntime runtime)
            {
                throw new NotImplementedException();
            }

            public void StartListeners(IJasperRuntime root, ITransportRuntime runtime)
            {
                throw new NotImplementedException();
            }


            public ISender CreateSender(Uri uri, CancellationToken cancellation, IJasperRuntime root)
            {
                throw new NotImplementedException();
            }

            public Endpoint GetOrCreateEndpoint(Uri? uri)
            {
                throw new NotImplementedException();
            }

            public Endpoint TryGetEndpoint(Uri? uri)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<Endpoint> Endpoints()
            {
                throw new NotImplementedException();
            }

            public void Initialize(IJasperRuntime root)
            {
                throw new NotImplementedException();
            }
        }
    }

}
