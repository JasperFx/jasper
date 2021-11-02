using System;
using System.Linq;
using Jasper.Configuration;
using Jasper.Runtime.Routing;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Stub;
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
                x.Endpoints.ListenForMessagesFrom("local://one").Sequential().Named("one");
                x.Endpoints.ListenForMessagesFrom("local://two").MaximumThreads(11);
                x.Endpoints.ListenForMessagesFrom("local://three").DurablyPersistedLocally();
                x.Endpoints.ListenForMessagesFrom("local://four").DurablyPersistedLocally().BufferedInMemory();
                x.Endpoints.ListenForMessagesFrom("local://five").ProcessInline();

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

        private StubTransport theStubTransport => theOptions.Endpoints.As<TransportCollection>().Get<StubTransport>();

        public void Dispose()
        {
            _host.Dispose();
        }

        [Fact]
        public void can_set_the_endpoint_name()
        {
            localQueue("one").Name.ShouldBe("one");
        }

        [Fact]
        public void publish_all_adds_an_all_subscription_to_the_endpoint()
        {
            theOptions.Endpoints.PublishAllMessages()
                .To("stub://5555");

            findEndpoint("stub://5555")
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
        public void configure_durable_queue()
        {
            theOptions.Endpoints.DurableScheduledMessagesLocalQueue
                .MaximumThreads(22);

            localQueue(TransportConstants.Durable)
                .ExecutionOptions.MaxDegreeOfParallelism
                .ShouldBe(22);
        }

        [Fact]
        public void mark_durable()
        {
            theOptions.Endpoints.ListenForMessagesFrom("stub://1111")
                .DurablyPersistedLocally();

            var endpoint = findEndpoint("stub://1111");
            endpoint.ShouldNotBeNull();
            endpoint.Mode.ShouldBe(EndpointMode.Durable);
            endpoint.IsListener.ShouldBeTrue();

        }

        [Fact]
        public void prefer_listener()
        {
            theOptions.Endpoints.ListenForMessagesFrom("stub://1111");
            theOptions.Endpoints.ListenForMessagesFrom("stub://2222");
            theOptions.Endpoints.ListenForMessagesFrom("stub://3333").UseForReplies();

            findEndpoint("stub://1111").IsUsedForReplies.ShouldBeFalse();
            findEndpoint("stub://2222").IsUsedForReplies.ShouldBeFalse();
            findEndpoint("stub://3333").IsUsedForReplies.ShouldBeTrue();
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
        public void configure_process_inline()
        {
            theOptions
                .Endpoints
                .ListenForMessagesFrom("local://three")
                .ProcessInline();


            localQueue("three")
                .Mode
                .ShouldBe(EndpointMode.Inline);
        }

        [Fact]
        public void configure_durable()
        {
            theOptions
                .Endpoints
                .ListenForMessagesFrom("local://three")
                .DurablyPersistedLocally();


            localQueue("three")
                .Mode
                .ShouldBe(EndpointMode.Durable);
        }

        [Fact]
        public void configure_not_durable()
        {
            theOptions.Endpoints.ListenForMessagesFrom("local://four");

            localQueue("four")
                .Mode
                .ShouldBe(EndpointMode.BufferedInMemory);
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
            var uriString = "stub://1111";
            theOptions.Endpoints.ListenForMessagesFrom(uriString);

            findEndpoint(uriString)
                .IsListener.ShouldBeTrue();
        }


        [Fact]
        public void select_reply_endpoint_with_one_listener()
        {
            theOptions.Endpoints.ListenForMessagesFrom("stub://2222");
            theOptions.Endpoints.PublishAllMessages().To("stub://3333");

            theStubTransport.ReplyEndpoint()
                .Uri.ShouldBe("stub://2222".ToUri());
        }

        [Fact]
        public void select_reply_endpoint_with_multiple_listeners_and_one_designated_reply_endpoint()
        {
            theOptions.Endpoints.ListenForMessagesFrom("stub://2222");
            theOptions.Endpoints.ListenForMessagesFrom("stub://4444").UseForReplies();
            theOptions.Endpoints.ListenForMessagesFrom("stub://5555");
            theOptions.Endpoints.PublishAllMessages().To("stub://3333");

            theStubTransport.ReplyEndpoint()
                .Uri.ShouldBe("stub://4444".ToUri());
        }

        [Fact]
        public void select_reply_endpoint_with_no_listeners()
        {
            theOptions.Endpoints.PublishAllMessages().To("stub://3333");
            theStubTransport.ReplyEndpoint().ShouldBeNull();
        }

    }
}
