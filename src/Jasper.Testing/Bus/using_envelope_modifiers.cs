﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class using_envelope_modifiers : IntegrationContext
    {
        public using_envelope_modifiers()
        {
            EnvelopeCatchingHandler.Received.Clear();

            with(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<EnvelopeCatchingHandler>();

                _.Publish.Message<Message1>().To("loopback://one").ModifyWith<FooModifier>().ModifyWith<BarModifier>();
                _.Publish.Message<Message2>().To("loopback://two");

            });


        }

        [Fact]
        public async Task applies_modifiers_per_channel()
        {
            await Bus.SendAndWait(new Message1());
            await Bus.SendAndWait(new Message2());

            var envelopeForChannelOne = EnvelopeCatchingHandler.Received.First(x => x.MessageType == typeof(Message1).ToMessageAlias());
            var envelopeForChannelTwo = EnvelopeCatchingHandler.Received.First(x => x.MessageType == typeof(Message2).ToMessageAlias());

            envelopeForChannelOne.Headers["foo"].ShouldBe("yes");
            envelopeForChannelOne.Headers["bar"].ShouldBe("yes");

            envelopeForChannelTwo.Headers.ContainsKey("foo").ShouldBeFalse();
            envelopeForChannelTwo.Headers.ContainsKey("bar").ShouldBeFalse();
        }
    }

    public class EnvelopeCatchingHandler
    {
        public readonly static IList<Envelope> Received = new List<Envelope>();

        public void Handle(Message1 message, Envelope envelope)
        {
            Received.Add(envelope);
        }

        public void Handle(Message2 message, Envelope envelope)
        {
            Received.Add(envelope);
        }
    }

    public class FooModifier : IEnvelopeModifier
    {
        public void Modify(Envelope envelope)
        {
            if (envelope.Headers.ContainsKey("foo"))
            {
                envelope.Headers["foo"] = "yes";
            }
            else
            {
                envelope.Headers.Add("foo", "yes");
            }

        }
    }

    public class BarModifier : IEnvelopeModifier
    {
        public void Modify(Envelope envelope)
        {
            if (envelope.Headers.ContainsKey("bar"))
            {
                envelope.Headers["bar"] = "yes";
            }
            else
            {
                envelope.Headers.Add("bar", "yes");
            }

        }
    }
}
