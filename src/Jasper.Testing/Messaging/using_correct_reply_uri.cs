using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class using_correct_reply_uri : SendingContext
    {
        [Fact]
        public async Task no_global_reply_uri()
        {
            await StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(7000);
            });

            await StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(7001);

                _.Publish.Message<Request>().To("tcp://localhost:7000");
            });

            var waiter = theTracker.WaitFor<Request>();
            await theSender.Messaging.Send(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe($"tcp://{Environment.MachineName}:7001".ToUri());
        }

        [Fact]
        public async Task with_a_global_reply_uri()
        {
            await StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(7002);
            });

            await StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(7003);

                _.Publish.Message<Request>().To("tcp://localhost:7002");

                _.Subscribe.At("tcp://balancer:7003");
            });

            var waiter = theTracker.WaitFor<Request>();
            await theSender.Messaging.Send(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe("tcp://balancer:7003".ToUri());
        }

        [Fact]
        public async Task with_a_global_reply_uri_but_overriding_the_reply_uri()
        {
            await StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(7005);

                _.Publish.Message<Request>().To("tcp://localhost:7004");

                _.Subscribe.At("tcp://balancer:7005");
            });

            await StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(7004);
            });



            var explicitReplyUri = "tcp://localhost:7006".ToUri();

            var waiter = theTracker.WaitFor<Request>();
            await theSender.Messaging.Send(new Request(), e =>
            {
                e.ReplyUri = explicitReplyUri;
            });

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe(explicitReplyUri);
        }

        [Fact]
        public async Task with_a_global_reply_uri_but_using_request_reply()
        {
            await StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(7008);

                _.Publish.Message<Request>().To("tcp://localhost:7007");

                _.Subscribe.At("tcp://balancer:7008");
            });

            await StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(7007);
            });

            var waiter = theTracker.WaitFor<Request>();
            await theSender.Messaging.Request<Response>(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe($"tcp://{Environment.MachineName}:7008".ToUri());
        }

        [Fact]
        public async Task with_a_global_reply_uri_using_global_request_reply()
        {
            await StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(7010);

                _.Publish.Message<Request>().To("tcp://localhost:7009");

                _.Subscribe.At("tcp://balancer:7010");
            });

            await StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(7009);
            });

            var waiter = theTracker.WaitFor<Request>();
            await theSender.Messaging.SendAndExpectResponseFor<Response>(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe("tcp://balancer:7010".ToUri());
        }

        [Fact]
        public async Task without_a_global_reply_uri_using_global_request_reply()
        {
            await StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(7012);

                _.Publish.Message<Request>().To("tcp://localhost:7011");

            });

            await StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(7011);
            });

            var waiter = theTracker.WaitFor<Request>();
            await theSender.Messaging.SendAndExpectResponseFor<Response>(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe($"tcp://{Environment.MachineName}:7012".ToUri());
        }
    }

    public class Request{}
    public class Response{}

    public class ResponseHandler
    {
        public Response Handle(Request request, Envelope envelope, MessageTracker tracker)
        {
            tracker.Record(request, envelope);

            return new Response();
        }
    }
}
