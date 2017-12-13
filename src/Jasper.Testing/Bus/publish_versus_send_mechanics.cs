using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Stub;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class publish_versus_send_mechanics : IDisposable
    {
        private JasperRuntime theRuntime;

        public publish_versus_send_mechanics()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Publish.Message<Message1>().To("stub://one");
                _.Publish.Message<Message2>().To("stub://one");
                _.Publish.Message<Message2>().To("stub://two");

                _.Services.AddSingleton<ITransport, StubTransport>();
            });
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }


        [Fact]
        public async Task send_with_known_subscribers()
        {
            await theRuntime.Bus.Send(new Message1());
            await theRuntime.Bus.Send(new Message2());

            var sent = theRuntime.AllSentThroughTheStubTransport();

            sent.Length.ShouldBe(3);
        }

        [Fact]
        public async Task send_message_with_no_known_subscribers()
        {
            await Exception<NoRoutesException>.ShouldBeThrownByAsync(async () =>
                await theRuntime.Bus.Send(new Message3()));
        }

        [Fact]
        public async Task publish_message_with_no_known_subscribers()
        {
            await theRuntime.Bus.Publish(new Message3());

            theRuntime.AllSentThroughTheStubTransport().Any().ShouldBeFalse();
        }

        [Fact]
        public async Task publish_with_known_subscribers()
        {
            await theRuntime.Bus.Publish(new Message1());
            await theRuntime.Bus.Publish(new Message2());

            var sent = theRuntime.AllSentThroughTheStubTransport();

            sent.Single(x => x.MessageType == typeof(Message1).ToMessageAlias()).Destination
                .ShouldBe("stub://one".ToUri());

            sent.Where(x => x.MessageType == typeof(Message2).ToMessageAlias())
                .Select(x => x.Destination)
                .ShouldHaveTheSameElementsAs("stub://one".ToUri(), "stub://two".ToUri());
        }
    }
}
