using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class missing_handlers
    {

        [Fact]
        public async Task calls_all_the_missing_handlers()
        {
            using var host = JasperHost.For(x =>
            {
                x.Services.AddSingleton<IMissingHandler, RecordingMissingHandler>();
                x.Services.AddSingleton<IMissingHandler, RecordingMissingHandler2>();
                x.Services.AddSingleton<IMissingHandler, RecordingMissingHandler3>();
            });

            var message = new MessageWithNoHandler();


            await host.ExecuteAndWaitAsync(x => x.EnqueueAsync(message));

            for (int i = 0; i < 4; i++)
            {
                if (RecordingMissingHandler.Recorded.Any())
                {
                    break;
                }

                await Task.Delay(250);
            }

            RecordingMissingHandler.Recorded.Single().Message.ShouldBeSameAs(message);
            RecordingMissingHandler2.Recorded.Single().Message.ShouldBeSameAs(message);
            RecordingMissingHandler3.Recorded.Single().Message.ShouldBeSameAs(message);
        }

        public class RecordingMissingHandler : IMissingHandler
        {
            public static IList<Envelope> Recorded = new List<Envelope>();

            public Task HandleAsync(Envelope? envelope, IJasperRuntime root)
            {
                Recorded.Add(envelope);

                root.ShouldNotBeNull();

                return Task.CompletedTask;
            }
        }

        public class RecordingMissingHandler2 : IMissingHandler
        {
            public static IList<Envelope> Recorded = new List<Envelope>();

            public Task HandleAsync(Envelope? envelope, IJasperRuntime root)
            {
                Recorded.Add(envelope);

                root.ShouldNotBeNull();

                return Task.CompletedTask;
            }
        }

        public class RecordingMissingHandler3 : IMissingHandler
        {
            public static IList<Envelope> Recorded = new List<Envelope>();

            public Task HandleAsync(Envelope? envelope, IJasperRuntime root)
            {
                Recorded.Add(envelope);

                root.ShouldNotBeNull();

                return Task.CompletedTask;
            }
        }

        public class MessageWithNoHandler
        {

        }
    }
}
