using System.Threading.Tasks;
using Jasper.RabbitMQ;
using Xunit;
using Xunit.Abstractions;

namespace PerformanceTests
{
    public class RabbitMqPerformance : PerformanceHarness
    {
        public static int Count;

        public RabbitMqPerformance(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task simple_publish()
        {
            var outboundName = $"sender{++Count}";
            var replyName = $"replies{++Count}";

            startTheSender(opts =>
            {

                opts.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue(outboundName);
                    x.DeclareQueue(replyName);
                    x.AutoProvision = true;
                });

                opts.Endpoints.ListenToRabbitQueue(replyName)
                    .ProcessInline().ListenerCount(3);

                opts.Endpoints.PublishAllMessages().ToRabbit(outboundName);
            });

            await time(() => sendMessages(100, 10));
        }

        [Fact]
        public async Task time_the_receiver()
        {
            var outboundName = $"sender{++Count}";
            var replyName = $"replies{++Count}";

            startTheSender(opts =>
            {

                opts.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue(outboundName);
                    x.DeclareQueue(replyName);
                    x.AutoProvision = true;
                });

                opts.Endpoints.ListenToRabbitQueue(replyName).UseForReplies();

                opts.Endpoints.PublishAllMessages().ToRabbit(outboundName);
            });

            await sendMessages(100, 10);

            await time(async () =>
            {
                startTheReceiver(opts =>
                {
                    opts.Endpoints.ConfigureRabbitMq(x =>
                    {
                        x.ConnectionFactory.HostName = "localhost";
                    });

                    opts.Endpoints.ListenToRabbitQueue(outboundName)
                        .ProcessInline()
                        .ListenerCount(3)
                        .UseForReplies();
                });

                await waitForMessagesToBeProcessed(1000);
            });
        }


    }
}
