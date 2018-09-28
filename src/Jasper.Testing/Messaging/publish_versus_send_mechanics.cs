using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class publish_versus_send_mechanics : IDisposable
    {
        private JasperRuntime theRuntime;

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        private async Task buildRuntime()
        {
            theRuntime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Publish.Message<Message1>().To("stub://one");
                _.Publish.Message<Message2>().To("stub://one");
                _.Publish.Message<Message2>().To("stub://two");

                _.Services.AddSingleton<ITransport, StubTransport>();
            });
        }


        [Fact]
        public async Task send_with_known_subscribers()
        {
            await buildRuntime();
            await theRuntime.Messaging.Send(new Message1());
            await theRuntime.Messaging.Send(new Message2());

            var sent = theRuntime.AllSentThroughTheStubTransport();

            sent.Length.ShouldBe(3);
        }

        [Fact]
        public async Task send_message_with_no_known_subscribers()
        {
            await buildRuntime();
            await Exception<NoRoutesException>.ShouldBeThrownByAsync(async () =>
                await theRuntime.Messaging.Send(new Message3()));
        }

        [Fact]
        public async Task publish_message_with_no_known_subscribers()
        {
            await buildRuntime();
            await theRuntime.Messaging.Publish(new Message3());

            theRuntime.AllSentThroughTheStubTransport().Any().ShouldBeFalse();
        }

        [Fact]
        public async Task publish_with_known_subscribers()
        {
            await buildRuntime();

            await theRuntime.Messaging.Publish(new Message1());
            await theRuntime.Messaging.Publish(new Message2());

            var sent = theRuntime.AllSentThroughTheStubTransport();

            sent.Single(x => x.MessageType == typeof(Message1).ToMessageTypeName()).Destination
                .ShouldBe("stub://one".ToUri());

            sent.Where(x => x.MessageType == typeof(Message2).ToMessageTypeName())
                .Select(x => x.Destination)
                .ShouldHaveTheSameElementsAs("stub://one".ToUri(), "stub://two".ToUri());
        }
    }
}
