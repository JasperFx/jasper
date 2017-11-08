using System;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Testing.Bus;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.Bus
{
    public class using_correct_reply_uri : SendingContext
    {
        public using_correct_reply_uri()
        {
            StartTheReceiver(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<ResponseHandler>();
                _.Transports.LightweightListenerAt(2444);
            });
        }

        [Fact]
        public void no_global_reply_uri()
        {
            StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(2555);

                _.Publish.Message<Request>().To("tcp://localhost:2444");
            });

            var waiter = theTracker.WaitFor<Request>();
            theSender.Bus.Send(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe($"tcp://{Environment.MachineName}:2555".ToUri());
        }

        [Fact]
        public void with_a_global_reply_uri()
        {
            StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(2555);

                _.Publish.Message<Request>().To("tcp://localhost:2444");

                _.Subscribe.At("tcp://balancer:2555");
            });

            var waiter = theTracker.WaitFor<Request>();
            theSender.Bus.Send(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe("tcp://balancer:2555".ToUri());
        }

        [Fact]
        public void with_a_global_reply_uri_but_overriding_the_reply_uri()
        {
            StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(2555);

                _.Publish.Message<Request>().To("tcp://localhost:2444");

                _.Subscribe.At("tcp://balancer:2555");
            });

            var explicitReplyUri = "tcp://localhost:3333".ToUri();

            var waiter = theTracker.WaitFor<Request>();
            theSender.Bus.Send(new Request(), e =>
            {
                e.ReplyUri = explicitReplyUri;
            });

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe(explicitReplyUri);
        }

        [Fact]
        public void with_a_global_reply_uri_but_using_request_reply()
        {
            StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(2555);

                _.Publish.Message<Request>().To("tcp://localhost:2444");

                _.Subscribe.At("tcp://balancer:2555");
            });

            var waiter = theTracker.WaitFor<Request>();
            theSender.Bus.Request<Response>(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe($"tcp://{Environment.MachineName}:2555".ToUri());
        }

        [Fact]
        public void with_a_global_reply_uri_using_global_request_reply()
        {
            StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(2555);

                _.Publish.Message<Request>().To("tcp://localhost:2444");

                _.Subscribe.At("tcp://balancer:2555");
            });

            var waiter = theTracker.WaitFor<Request>();
            theSender.Bus.SendAndExpectResponseFor<Response>(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe("tcp://balancer:2555".ToUri());
        }

        [Fact]
        public void without_a_global_reply_uri_using_global_request_reply()
        {
            StartTheSender(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Transports.LightweightListenerAt(2555);

                _.Publish.Message<Request>().To("tcp://localhost:2444");

            });

            var waiter = theTracker.WaitFor<Request>();
            theSender.Bus.SendAndExpectResponseFor<Response>(new Request());

            waiter.Wait(5.Seconds());
            waiter.IsCompleted.ShouldBeTrue();

            waiter.Result.ReplyUri.ShouldBe($"tcp://{Environment.MachineName}:2555".ToUri());
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
