using System;
using System.Linq;
using Jasper.Configuration;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Util;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class configuring_endpoints : IDisposable
    {
        private IHost _host;
        private JasperOptions theOptions;

        public configuring_endpoints()
        {
            _host = Host.CreateDefaultBuilder().UseJasper(x =>
            {
                x.Endpoints.ListenForMessagesFrom("local://one").Sequential();
                x.Endpoints.ListenForMessagesFrom("local://two").MaximumThreads(11);
                x.Endpoints.ListenForMessagesFrom("local://three").Durably();
                x.Endpoints.ListenForMessagesFrom("local://four").Durably().Lightweight();

            }).Build();

            theOptions = _host.Get<JasperOptions>();
        }

        private LocalQueueSettings localQueue(string queueName)
        {
            return theOptions.Endpoints.As<TransportCollection>().Get<LocalTransport>()
                .QueueFor(queueName);

        }

        private Endpoint findEndpoint(string uri)
        {
            return theOptions.Endpoints.As<TransportCollection>()
                .TryGetEndpoint(uri.ToUri());
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        [Fact]
        public void publish_all_adds_an_all_subscription_to_the_endpoint()
        {
            theOptions.Endpoints.PublishAllMessages()
                .ToPort(5555);

            findEndpoint("tcp://localhost:5555")
                .Subscriptions.Single()
                .Scope.ShouldBe(RoutingScope.All);
        }

        [Fact]
        public void configure_default_queue()
        {
            theOptions.Endpoints.DefaultLocalQueue
                .MaximumThreads(13);

            localQueue(TransportConstants.Default)
                .ExecutionOptions.MaxDegreeOfParallelism
                .ShouldBe(13);
        }

        [Fact]
        public void listen_for_port()
        {
            theOptions.Endpoints.ListenAtPort(1111)
                .Durably();

            var endpoint = findEndpoint("tcp://localhost:1111");
            endpoint.ShouldNotBeNull();
            endpoint.IsDurable.ShouldBeTrue();
            endpoint.IsListener.ShouldBeTrue();

        }

        [Fact]
        public void configure_sequential()
        {
            localQueue("one")
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);
        }

        [Fact]
        public void configure_max_parallelization()
        {
            localQueue("two")
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);
        }

        [Fact]
        public void configure_durable()
        {
            theOptions
                .Endpoints
                .ListenForMessagesFrom("local://three")
                .Durably();


            localQueue("three")
                .IsDurable
                .ShouldBeTrue();
        }

        [Fact]
        public void configure_not_durable()
        {
            theOptions.Endpoints.ListenForMessagesFrom("local://four");

            localQueue("four")
                .IsDurable
                .ShouldBeFalse();
        }

        [Fact]
        public void configure_execution()
        {
            theOptions.Endpoints.LocalQueue("foo")
                .ConfigureExecution(x => x.BoundedCapacity = 111);

            localQueue("foo")
                .ExecutionOptions.BoundedCapacity.ShouldBe(111);
        }

        [Fact]
        public void sets_is_listener()
        {
            var uriString = "tcp://localhost:1111";
            theOptions.Endpoints.ListenForMessagesFrom(uriString);

            findEndpoint(uriString)
                .IsListener.ShouldBeTrue();
        }

    }
}
