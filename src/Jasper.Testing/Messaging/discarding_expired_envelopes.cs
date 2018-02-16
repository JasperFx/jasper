using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Messaging
{
    [Collection("integration")]
    public class discarding_expired_envelopes
    {
        [Fact]
        public async Task can_discard_an_envelope_if_expired()
        {
            var sink = Substitute.For<IMessageEventSink>();

            using (var runtime = JasperRuntime.For(x =>
            {
                x.Handlers.DisableConventionalDiscovery();
                x.Logging.LogMessageEventsWith(sink);
            }))
            {
                var pipeline = runtime.Get<IHandlerPipeline>();

                var envelope = ObjectMother.Envelope();
                envelope.DeliverBy = DateTime.UtcNow.Subtract(1.Minutes());
                envelope.Callback = Substitute.For<IMessageCallback>();

                await pipeline.Invoke(envelope);

                // Log the discard
                sink.Received().DiscardedEnvelope(envelope);


                envelope.Callback.Received().MarkComplete();


            }
        }
    }
}
