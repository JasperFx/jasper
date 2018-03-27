using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class missing_handler_processing : IntegrationContext
    {
        [Fact]
        public async Task missing_handlers_are_called()
        {
             NoMessageHandler1.Reset();
             NoMessageHandler2.Reset();

            await with(r =>
            {
                r.Services.AddTransient<IMissingHandler, NoMessageHandler1>();
                r.Services.AddTransient<IMissingHandler, NoMessageHandler2>();

                // Hack until we get a default queue
                r.Publish.Message<MessageWithNoHandler>().To("loopback://incoming");
            });

            var msg1 = new MessageWithNoHandler();

            await Bus.SendAndWait(msg1);

            await NoMessageHandler1.Finished;
            await NoMessageHandler2.Finished;

            NoMessageHandler1.Handled.Single().Message.ShouldBe(msg1);
            NoMessageHandler2.Handled.Single().Message.ShouldBe(msg1);
        }
    }

    public class NoMessageHandler1 : IMissingHandler
    {
        public static readonly List<Envelope> Handled = new List<Envelope>();
        private static TaskCompletionSource<Envelope> _source;

        public static void Reset()
        {
            Handled.Clear();
            _source = new TaskCompletionSource<Envelope>();
        }

        public static Task<Envelope> Finished => _source.Task;

        public Task Handle(Envelope envelope, IMessageContext context)
        {
            _source.SetResult(envelope);
            Handled.Add(envelope);

            return Task.CompletedTask;
        }
    }

    public class NoMessageHandler2 : IMissingHandler
    {
        public static readonly List<Envelope> Handled = new List<Envelope>();
        private static TaskCompletionSource<Envelope> _source;

        public static void Reset()
        {
            Handled.Clear();
            _source = new TaskCompletionSource<Envelope>();
        }

        public static Task<Envelope> Finished => _source.Task;

        public Task Handle(Envelope envelope, IMessageContext context)
        {
            _source.SetResult(envelope);
            Handled.Add(envelope);

            return Task.CompletedTask;
        }
    }

    public class MessageWithNoHandler
    {

    }
}
