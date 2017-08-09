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
                _.Channels.ListenForMessagesFrom("loopback://one");
                _.Channels.ListenForMessagesFrom("loopback://two");
                _.Channels.ListenForMessagesFrom("loopback://three");

                _.Channels["loopback://two"].UseAsControlChannel();
            });

            var controlChannel = channels().ControlChannel;

            controlChannel.Uri.ShouldBe("loopback://two".ToUri());
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
                _.Channels.ListenForMessagesFrom("loopback://one");
                _.Channels.ListenForMessagesFrom("loopback://two")
                    .DeliveryFastWithoutGuarantee();
                _.Channels.ListenForMessagesFrom("loopback://three");

            });

            var channelGraph = channels();
            channelGraph["loopback://two".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryFastWithoutGuarantee);

            channelGraph["loopback://one".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);

            channelGraph["loopback://three".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);
        }

        [Fact]
        public void add_envelope_modifiers()
        {
            with(_ =>
            {
                _.Messaging.Send<Message1>().To("loopback://one");
                _.Messaging.Send<Message2>().To("loopback://two");
                _.Messaging.Send<Message3>().To("loopback://three");

                _.Channels["loopback://two"]
                    .ModifyWith<FakeModifier>()
                    .ModifyWith(new FakeModifier2());
            });

            var channelGraph = channels();

            channelGraph["loopback://one".ToUri()].Modifiers.Any().ShouldBeFalse();
            channelGraph["loopback://three".ToUri()].Modifiers.Any().ShouldBeFalse();

            channelGraph["loopback://two".ToUri()].Modifiers.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(FakeModifier), typeof(FakeModifier2));
        }

        [Fact]
        public void set_accepted_content_types()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.Messaging.Send<Message1>().To("loopback://one");
                _.Messaging.Send<Message2>().To("loopback://two");
                _.Messaging.Send<Message3>().To("loopback://three");

                _.Channels["loopback://two"].AcceptedContentTypes("text/xml");
            });

            channels()["loopback://two"].AcceptedContentTypes.Single()
                .ShouldBe("text/xml");

            // Bad test
        }

        [Fact]
        public void set_default_content_type()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.Messaging.Send<Message1>().To("loopback://one");
                _.Messaging.Send<Message2>().To("loopback://two");
                _.Messaging.Send<Message3>().To("loopback://three");

                _.Channels["loopback://two"].AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/two");

                _.Channels["loopback://three"]
                    .AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/one");
            });

            channels()["loopback://two"].AcceptedContentTypes
                .ShouldHaveTheSameElementsAs("fake/two", "application/json", "fake/one");

            channels()["loopback://three"].AcceptedContentTypes.First()
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
