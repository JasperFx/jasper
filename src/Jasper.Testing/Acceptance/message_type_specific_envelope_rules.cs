using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Attributes;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class MySpecialMessageHandler
    {
        public void Handle(MySpecialMessage message){}
    }

    public class message_type_specific_envelope_rules : IntegrationContext
    {
        public message_type_specific_envelope_rules(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void apply_message_type_rules_from_attributes()
        {
            var router = Host.Get<IEnvelopeRouter>();

            var envelope = router.RouteOutgoingByMessage(new MySpecialMessage()).Single();

            envelope.Headers["special"].ShouldBe("true");
        }


        [Fact]
        public void deliver_by_mechanics()
        {
            var router = Host.Get<IEnvelopeRouter>();

            var envelope = router.RouteOutgoingByMessage(new MySpecialMessage())
                .Single();

            envelope.DeliverBy.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        }


        [Fact]
        public async Task see_the_customizations_happen_inside_of_message_context()
        {
            var context = Host.Get<IExecutionContext>();

            // Just to force the message context to pool up the envelope instead
            // of sending it out
            context.UseInMemoryTransactionAsync();

            var mySpecialMessage = new MySpecialMessage();

            await context.SendToDestinationAsync("stub://2001".ToUri(), mySpecialMessage);

            var outgoing = context.As<ExecutionContext>().Outstanding.Single();

            outgoing.Headers["special"].ShouldBe("true");
        }
    }

    public class SpecialAttribute : ModifyEnvelopeAttribute
    {
        public override void Modify(Envelope envelope)
        {
            envelope.Headers.Add("special", "true");
        }
    }

    [Special]
    [DeliverWithin(5)]
    public class MySpecialMessage
    {
    }

    #region sample_UsingDeliverWithinAttribute
    // Any message of this type should be successfully
    // delivered within 10 seconds or discarded
    [DeliverWithin(10)]
    public class StatusMessage
    {
    }

    #endregion
}
