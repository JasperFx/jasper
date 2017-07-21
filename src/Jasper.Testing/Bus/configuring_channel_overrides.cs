using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class configuring_channel_overrides : IntegrationContext
    {
        [Fact]
        public void explicitly_establish_the_control_channel()
        {
            with(_ =>
            {
                _.Channels.ListenForMessagesFrom("memory://one");
                _.Channels.ListenForMessagesFrom("memory://two");
                _.Channels.ListenForMessagesFrom("memory://three");

                _.Channels["memory://two"].UseAsControlChannel();
            });

            var controlChannel = channels().ControlChannel;

            controlChannel.Uri.ShouldBe("memory://two".ToUri());
        }

        private ChannelGraph channels()
        {
            return Runtime.Container.GetInstance<ChannelGraph>();
        }

        [Fact]
        public void configure_delivery_mode()
        {
            with(_ =>
            {
                _.Channels.ListenForMessagesFrom("memory://one");
                _.Channels.ListenForMessagesFrom("memory://two")
                    .DeliveryFastWithoutGuarantee();
                _.Channels.ListenForMessagesFrom("memory://three");

            });

            var channelGraph = channels();
            channelGraph["memory://two".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryFastWithoutGuarantee);

            channelGraph["memory://one".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);

            channelGraph["memory://three".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);
        }

        [Fact]
        public void add_envelope_modifiers()
        {
            with(_ =>
            {
                _.Messages.SendMessage<Message1>().To("memory://one");
                _.Messages.SendMessage<Message2>().To("memory://two");
                _.Messages.SendMessage<Message3>().To("memory://three");

                _.Channels["memory://two"]
                    .ModifyWith<FakeModifier>()
                    .ModifyWith(new FakeModifier2());
            });

            var channelGraph = channels();

            channelGraph["memory://one".ToUri()].Modifiers.Any().ShouldBeFalse();
            channelGraph["memory://three".ToUri()].Modifiers.Any().ShouldBeFalse();

            channelGraph["memory://two".ToUri()].Modifiers.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(FakeModifier), typeof(FakeModifier2));
        }

        [Fact]
        public void set_accepted_content_types()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.Messages.SendMessage<Message1>().To("memory://one");
                _.Messages.SendMessage<Message2>().To("memory://two");
                _.Messages.SendMessage<Message3>().To("memory://three");

                _.Channels["memory://two"].AcceptedContentTypes("text/xml");
            });

            channels()["memory://two"].AcceptedContentTypes.Single()
                .ShouldBe("text/xml");

            // Bad test
        }

        [Fact]
        public void set_default_content_type()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.Messages.SendMessage<Message1>().To("memory://one");
                _.Messages.SendMessage<Message2>().To("memory://two");
                _.Messages.SendMessage<Message3>().To("memory://three");

                _.Channels["memory://two"].AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/two");

                _.Channels["memory://three"]
                    .AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/one");
            });

            channels()["memory://two"].AcceptedContentTypes
                .ShouldHaveTheSameElementsAs("fake/two", "application/json", "fake/one");

            channels()["memory://three"].AcceptedContentTypes.First()
                .ShouldBe("fake/one");
        }

    }

    public class FakeModifier : IEnvelopeModifier
    {
        public void Modify(Envelope envelope)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FakeModifier2 : FakeModifier
    {

    }
}
