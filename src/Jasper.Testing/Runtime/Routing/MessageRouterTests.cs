using System;
using Jasper.Attributes;
using Jasper.Runtime.Routing;
using Jasper.Transports;
using Jasper.Transports.Local;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestMessages;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.Testing.Runtime.Routing
{
    public class MessageRouterFixture : IDisposable
    {
        public MessageRouterFixture()
        {
            theHost = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Handlers
                        .DisableConventionalDiscovery()
                        .Discovery(x => x.IncludeType<MessageReceivingGuy>());

                    opts.Endpoints.Publish(x =>
                    {
                        x.Message<Message2>().ToPort(1111);
                        x.Message<Message3>().ToPort(1111);

                        x.ToLocalQueue("configured");
                    });
                })
                .Start();
        }

        public IHost theHost { get; }

        public void Dispose()
        {
            theHost?.Dispose();
        }
    }

    public class MessageRouterTests : IClassFixture<MessageRouterFixture>
    {
        private IMessageRouter theRouter;

        public MessageRouterTests(MessageRouterFixture fixture)
        {
            theRouter = fixture.theHost.Get<IMessageRouter>();
        }

        [Theory]
        // Default behavior
        [InlineData(typeof(Message1), TransportConstants.Default)]

        // LocalQueue attribute wins
        [InlineData(typeof(DecoratedMessage), "foo")]

        // Take advantage of publishing rules
        [InlineData(typeof(Message2), "configured")]
        [InlineData(typeof(Message3), "configured")]
        public void select_local_route(Type messageType, string expected)
        {
            var route = theRouter.CreateLocalRoute(messageType);
            var actual =  LocalTransport.QueueName(route.Destination);
            actual.ShouldBe(expected);
        }
    }

    public class MessageReceivingGuy
    {
        public void Handle(Message1 message)
        {

        }

        public void Handle(Message2 message)
        {

        }
    }


    [LocalQueue("foo")]
    public class DecoratedMessage
    {

    }

    // SAMPLE: local-queue-routed-message

    // This message type would go to the "important"
    // named queue
    [LocalQueue("important")]
    public class ImportantMessage
    {

    }
    // ENDSAMPLE
}
