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

namespace Jasper.Testing.Runtime
{
    public class publish_versus_send_mechanics : IntegrationContext
    {
        public publish_versus_send_mechanics(DefaultApp @default) : base(@default)
        {
            with(_ =>
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
            var session = await Host.ExecuteAndWait(x => x.Publish(new Message3()));

            session.AllRecordsInOrder().Any(x => x.EventType != EventType.NoRoutes).ShouldBeFalse();
        }

        [Fact]
        public async Task publish_with_known_subscribers()
        {
            var session = await Host.ExecuteAndWait(async c =>
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
            await Should.ThrowAsync<NoRoutesException>(async () =>
                await Publisher.Send(new Message3()));
        }


        [Fact]
        public async Task send_with_known_subscribers()
        {
            var session = await Host.ExecuteAndWait(async c =>
            {
                await c.Send(new Message1());
                await c.Send(new Message2());

            });

            session.AllRecordsInOrder(EventType.Sent)
                .Length.ShouldBe(3);
        }
    }
}
