using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Tracking;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class tracking_correlation_id
    {
        public class ExecutedMessage
        {
        }

        public class ExecutedMessageGuy
        {
            public static void Handle(ExecutedMessage message)
            {
            }
        }

        [Fact]
        public async Task tracking_correlation_id_on_everything()
        {
            var host = JasperHost.For(x =>
            {
                x.Handlers.DisableConventionalDiscovery().IncludeType<ExecutedMessageGuy>();
                x.Endpoints.PublishAllMessages().To("local://outgoing");

                x.Extensions.UseMessageTrackingTestingSupport();
            });

            try
            {
                var id2 = Guid.Empty;
                var session2 = await host.ExecuteAndWait(async context =>
                {
                    id2 = context.CorrelationId;

                    await context.Send(new ExecutedMessage());
                    await context.Publish(new ExecutedMessage());
                    await context.ScheduleSend(new ExecutedMessage(), DateTime.UtcNow.AddDays(5));
                });

                var envelopes = session2
                    .AllRecordsInOrder(EventType.Sent)
                    .Select(x => x.Envelope)
                    .ToArray();


                foreach (var envelope in envelopes) envelope.CorrelationId.ShouldBe(id2);
            }
            finally
            {
                host.Dispose();
            }
        }
    }
}
