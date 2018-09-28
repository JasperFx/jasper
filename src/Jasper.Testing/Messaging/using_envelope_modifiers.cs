using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging.Lightweight.Protocol;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    [Collection("integration")] // gotta get rid of the static EnvelopeCatchingHandler first
    public class using_envelope_modifiers : IntegrationContext
    {
        [Fact]
        public async Task applies_modifiers_per_channel()
        {
            var receiver = new EnvelopeReceiver();
            var tracker = new MessageTracker();

            await with(_ =>
            {
                _.Services.AddSingleton(tracker);
                _.Services.AddSingleton(receiver);

                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<EnvelopeCatchingHandler>();

                _.Publish.Message<Message1>().To("loopback://one").ModifyWith<FooModifier>().ModifyWith<BarModifier>();
                _.Publish.Message<Message2>().To("loopback://two");

            });

            var wait1 = tracker.WaitFor<Message1>();
            var wait2 = tracker.WaitFor<Message2>();

            await Bus.Send(new Message1());
            await Bus.Send(new Message2());

            await wait1;
            await wait2;

            var envelopeForChannelOne = receiver.Received.First(x => x.MessageType == typeof(Message1).ToMessageTypeName());
            var envelopeForChannelTwo = receiver.Received.First(x => x.MessageType == typeof(Message2).ToMessageTypeName());

            envelopeForChannelOne.Headers["foo"].ShouldBe("yes");
            envelopeForChannelOne.Headers["bar"].ShouldBe("yes");

            envelopeForChannelTwo.Headers.ContainsKey("foo").ShouldBeFalse();
            envelopeForChannelTwo.Headers.ContainsKey("bar").ShouldBeFalse();
        }
    }

    public class EnvelopeReceiver
    {
        public readonly IList<Envelope> Received = new List<Envelope>();
    }

    public class EnvelopeCatchingHandler
    {
        private EnvelopeReceiver _receiver;
        private readonly MessageTracker _tracker;

        public EnvelopeCatchingHandler(EnvelopeReceiver receiver, MessageTracker tracker)
        {
            _receiver = receiver;
            _tracker = tracker;
        }

        public void Handle(Message1 message, Envelope envelope)
        {
            _receiver.Received.Add(envelope);
            _tracker.Record(message, envelope);
        }

        public void Handle(Message2 message, Envelope envelope)
        {
            _receiver.Received.Add(envelope);
            _tracker.Record(message, envelope);
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
