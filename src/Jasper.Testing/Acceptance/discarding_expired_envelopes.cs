using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Transports;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class discarding_expired_envelopes
    {
        [Fact]
        public async Task can_discard_an_envelope_if_expired()
        {
            var logger = Substitute.For<IMessageLogger>();

            using (var runtime = JasperHost.For(x =>
            {
                x.Handlers.DisableConventionalDiscovery();
                x.Services.AddSingleton(logger);
            }))
            {
                var pipeline = runtime.Get<IHandlerPipeline>();

                var envelope = ObjectMother.Envelope();
                envelope.DeliverBy = DateTime.UtcNow.Subtract(1.Minutes());
                var channel = Substitute.For<IChannelCallback>();

                await pipeline.Invoke(envelope, channel);

                // Log the discard
                logger.Received().DiscardedEnvelope(envelope);


#pragma warning disable 4014
                channel.Received().Complete(envelope);
#pragma warning restore 4014
            }

        }
    }
}
