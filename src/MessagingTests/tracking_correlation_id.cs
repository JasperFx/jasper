using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using Xunit;

namespace MessagingTests
{


    public class tracking_correlation_id
    {
        [Fact]
        public async Task tracking_correlation_id_on_everything()
        {
            var catcher = new EnvelopeCatcher();

            var host = JasperHost.For(x =>
            {
                x.Handlers.DisableConventionalDiscovery().IncludeType<ExecutedMessageGuy>();
                x.Publish.AllMessagesTo("stub://outgoing");

                x.Services.AddSingleton(catcher);
            });

            try
            {
                var context = host.Get<IMessageContext>();

                await context.Invoke(new ExecutedMessage());
                catcher.Envelopes.Single().CorrelationId.ShouldBe(context.CorrelationId);

                await context.Send(new ExecutedMessage());
                await context.Publish(new ExecutedMessage());
                await context.ScheduleSend(new ExecutedMessage(), DateTime.UtcNow.AddDays(5));

                var envelopes = host.GetAllEnvelopesSent();

                foreach (var envelope in envelopes)
                {
                    envelope.CorrelationId.ShouldBe(context.CorrelationId);
                }
            }
            finally
            {
                host.Dispose();
            }
        }

        public class EnvelopeCatcher
        {
            public readonly IList<Envelope> Envelopes = new List<Envelope>();
        }

        public class ExecutedMessage
        {

        }

        public class ExecutedMessageGuy
        {
            public static void Handle(ExecutedMessage message, Envelope envelope, EnvelopeCatcher catcher)
            {
                catcher.Envelopes.Add(envelope);
            }
        }
    }
}
