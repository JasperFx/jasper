using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime.Routing;
using Jasper.Tracking;
using Jasper.Util;
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

namespace Jasper.Testing.Runtime
{
    public class publish_versus_send_mechanics : IDisposable
    {
        public void Dispose()
        {
            theHost?.Dispose();
        }

        private IHost theHost;

        private void buildHost()
        {
            theHost = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Endpoints.Publish(x => x
                    .Message<Message1>()
                    .Message<Message2>()
                    .ToLocalQueue("one"));

                _.Endpoints.Publish(x => x.Message<Message2>().ToLocalQueue("two"));

                _.Extensions.UseMessageTrackingTestingSupport();

            });
        }

        [Fact]
        public async Task publish_message_with_no_known_subscribers()
        {
            buildHost();

            var session = await theHost.ExecuteAndWait(x => x.Publish(new Message3()));

            session.AllRecordsInOrder().Any(x => x.EventType != EventType.NoRoutes).ShouldBeFalse();
        }

        [Fact]
        public async Task publish_with_known_subscribers()
        {
            buildHost();

            var session = await theHost.ExecuteAndWait(async c =>
            {
                await c.Publish(new Message1());
                await c.Publish(new Message2());

            });

            session
                .FindEnvelopesWithMessageType<Message1>(EventType.Sent)
                .Single()
                .Envelope.Destination
                .ShouldBe("local://one".ToUri());

            session
                .FindEnvelopesWithMessageType<Message2>(EventType.Sent)
                .Select(x => x.Envelope.Destination).OrderBy(x => x.ToString())
                .ShouldHaveTheSameElementsAs( "local://one".ToUri(), "local://two".ToUri());

        }

        [Fact]
        public async Task send_message_with_no_known_subscribers()
        {
            buildHost();
            await Should.ThrowAsync<NoRoutesException>(async () =>
                await theHost.Get<IMessagePublisher>().Send(new Message3()));
        }


        [Fact]
        public async Task send_with_known_subscribers()
        {
            buildHost();

            var session = await theHost.ExecuteAndWait(async c =>
            {
                await c.Send(new Message1());
                await c.Send(new Message2());

            });

            session.AllRecordsInOrder(EventType.Sent)
                .Length.ShouldBe(3);
        }
    }
}
