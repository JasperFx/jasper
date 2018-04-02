using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class message_type_specific_envelope_rules
    {
        [Fact]
        public void apply_message_type_rules_from_attributes()
        {
            var settings = new MessagingSettings();
            var envelope = new Envelope
            {
                Message = new MySpecialMessage()
            };

            settings.ApplyMessageTypeSpecificRules(envelope);

            envelope.Headers["special"].ShouldBe("true");
        }

        [Fact]
        public void apply_message_rules()
        {
            var rule1 = new MessageTypeRule(t => t == typeof(MySpecialMessage), e => e.Headers.Add("rule1", "true"));
            var rule2 = new MessageTypeRule(t => t.IsInNamespace(typeof(MySpecialMessage).Namespace), e => e.Headers.Add("rule2", "true"));

            var settings = new MessagingSettings();
            settings.MessageTypeRules.Add(rule1);
            settings.MessageTypeRules.Add(rule2);

            var envelope = new Envelope
            {
                Message = new MySpecialMessage()
            };

            settings.ApplyMessageTypeSpecificRules(envelope);

            envelope.Headers["rule1"].ShouldBe("true");
            envelope.Headers["rule2"].ShouldBe("true");

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
    public class MySpecialMessage
    {

    }
}
