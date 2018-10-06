using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class message_type_specific_envelope_rules
    {
        [Fact]
        public void apply_message_type_rules_from_attributes()
        {
            using (var runtime = JasperRuntime.Basic())
            {
                var root = runtime.Get<IMessagingRoot>();
                var envelope = new Envelope
                {
                    Message = new MySpecialMessage()
                };

                root.ApplyMessageTypeSpecificRules(envelope);

                envelope.Headers["special"].ShouldBe("true");
            }


        }


        [Fact]
        public async Task see_the_customizations_happen_inside_of_message_context()
        {
            var runtime = await JasperRuntime.BasicAsync();


            try
            {
                var context = runtime.Get<IMessageContext>();

                // Just to force the message context to pool up the envelope instead
                // of sending it out
                await context.EnlistInTransaction(new InMemoryEnvelopeTransaction());

                var mySpecialMessage = new MySpecialMessage();

                await context.Send("tcp://localhost:2001".ToUri(), mySpecialMessage);

                var outgoing = context.As<MessageContext>().Outstanding.Single();

                outgoing.Headers["special"].ShouldBe("true");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }





        [Fact]
        public void deliver_by_mechanics()
        {
            using (var runtime = JasperRuntime.Basic())
            {
                var root = runtime.Get<IMessagingRoot>();
                var envelope = new Envelope
                {
                    Message = new MySpecialMessage()
                };

                root.ApplyMessageTypeSpecificRules(envelope);

                envelope.DeliverBy.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
            }

        }


    }

    public class SpecialAttribute : ModifyEnvelopeAttribute
    {
        public override void Modify(Envelope envelope)
        {
            envelope.Headers.Add("special", "true");
        }
    }

    [Special, DeliverWithin(5)]
    public class MySpecialMessage
    {

    }

    // SAMPLE: UsingDeliverWithinAttribute
    // Any message of this type should be successfully
    // delivered within 10 seconds or discarded
    [DeliverWithin(10)]
    public class StatusMessage{}
    // ENDSAMPLE
}
