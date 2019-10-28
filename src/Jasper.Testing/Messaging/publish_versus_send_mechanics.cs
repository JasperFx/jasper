using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Stub;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class publish_versus_send_mechanics : IDisposable
    {
        public void Dispose()
        {
            theHost?.Dispose();
        }

        private IHost theHost;

        private void buildRuntime()
        {
            theHost = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Publish.Message<Message1>().To("stub://one");
                _.Publish.Message<Message2>().To("stub://one");
                _.Publish.Message<Message2>().To("stub://two");

            });
        }

        [Fact]
        public void publish_message_with_no_known_subscribers()
        {
            buildRuntime();
            theHost.Get<IMessagePublisher>().Publish(new Message3());

            theHost.GetAllEnvelopesSent().Any().ShouldBeFalse();
        }

        [Fact]
        public async Task publish_with_known_subscribers()
        {
            buildRuntime();

            await theHost.Get<IMessagePublisher>().Publish(new Message1());
            await theHost.Get<IMessagePublisher>().Publish(new Message2());

            var sent = theHost.GetAllEnvelopesSent();

            sent.Single(x => x.MessageType == typeof(Message1).ToMessageTypeName()).Destination
                .ShouldBe("stub://one".ToUri());

            sent.Where(x => x.MessageType == typeof(Message2).ToMessageTypeName())
                .Select(x => x.Destination)
                .ShouldHaveTheSameElementsAs("stub://one".ToUri(), "stub://two".ToUri());
        }

        [Fact]
        public async Task send_message_with_no_known_subscribers()
        {
            buildRuntime();
            await Should.ThrowAsync<NoRoutesException>(async () =>
                await theHost.Get<IMessagePublisher>().Send(new Message3()));
        }


        [Fact]
        public async Task send_with_known_subscribers()
        {
            buildRuntime();
            await theHost.Get<IMessagePublisher>().Send(new Message1());
            await theHost.Get<IMessagePublisher>().Send(new Message2());

            var sent = theHost.GetAllEnvelopesSent();

            sent.Length.ShouldBe(3);
        }
    }
}
