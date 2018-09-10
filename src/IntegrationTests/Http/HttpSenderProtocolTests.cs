using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests.Persistence.Marten;
using Jasper;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using TestMessages;
using Xunit;

namespace IntegrationTests.Http
{
    public class HttpSenderProtocolTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public async Task try_to_send_a_batch(int size)
        {
            TargetReceiver.Targets.Clear();

            var receiver = await JasperRuntime.ForAsync(x =>
            {
                x.Handlers
                    .DisableConventionalDiscovery()
                    .IncludeType<TargetReceiver>();

                x.Transports.Http.EnableListening(true);

                x.Hosting
                    .UseUrls("http://localhost:5508")
                    .UseKestrel();
            });

            var sender = await JasperRuntime.ForAsync(x =>
            {
                x.Publish.AllMessagesTo("http://localhost:5508/messages");
            });

            try
            {
                TargetReceiver.Targets.Clear();

                var targets = Target.GenerateRandomData(size).ToArray();

                foreach (var target in targets)
                {
                    await sender.Messaging.Send(target);
                }

                while (TargetReceiver.Targets.Count < size)
                {
                    await Task.Delay(250.Milliseconds());
                }

                TargetReceiver.Targets.Count.ShouldBe(size);
            }
            finally
            {
                await receiver.Shutdown();
                await sender.Shutdown();
            }
        }
    }

    public class TargetReceiver
    {
        public static readonly ConcurrentBag<Target> Targets = new ConcurrentBag<Target>();

        public static void Handle(Target target)
        {
            Targets.Add(target);
        }
    }
}
