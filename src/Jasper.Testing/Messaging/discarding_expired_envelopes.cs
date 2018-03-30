using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class discarding_expired_envelopes
    {
        [Fact]
        public async Task can_discard_an_envelope_if_expired()
        {
            var logger = Substitute.For<IMessageLogger>();

            var runtime = await JasperRuntime.ForAsync(x =>
            {
                x.Handlers.DisableConventionalDiscovery();
                x.Services.AddSingleton(logger);
            });

            try
            {
                var pipeline = runtime.Get<IHandlerPipeline>();

                var envelope = ObjectMother.Envelope();
                envelope.DeliverBy = DateTime.UtcNow.Subtract(1.Minutes());
                envelope.Callback = Substitute.For<IMessageCallback>();

                await pipeline.Invoke(envelope);

                // Log the discard
                logger.Received().DiscardedEnvelope(envelope);


#pragma warning disable 4014
                envelope.Callback.Received().MarkComplete();
#pragma warning restore 4014
            }
            finally
            {
                await runtime.Shutdown();
            }
        }
    }
}
