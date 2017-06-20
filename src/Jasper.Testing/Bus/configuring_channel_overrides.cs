using System.Linq;
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
                _.ListenForMessagesFrom("stub://one");
                _.ListenForMessagesFrom("stub://two");
                _.ListenForMessagesFrom("stub://three");

                _.Channel("stub://two").UseAsControlChannel();
            });

            var controlChannel = channels().ControlChannel;

            controlChannel.Uri.ShouldBe("stub://two".ToUri());
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
                _.ListenForMessagesFrom("stub://one");
                _.ListenForMessagesFrom("stub://two")
                    .DeliveryFastWithoutGuarantee();
                _.ListenForMessagesFrom("stub://three");

            });

            var channelGraph = channels();
            channelGraph["stub://two".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryFastWithoutGuarantee);

            channelGraph["stub://one".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);

            channelGraph["stub://three".ToUri()]
                .Mode.ShouldBe(DeliveryMode.DeliveryGuaranteed);
        }

        [Fact]
        public void add_envelope_modifiers()
        {
            with(_ =>
            {
                _.SendMessage<Message1>().To("stub://one");
                _.SendMessage<Message2>().To("stub://two");
                _.SendMessage<Message3>().To("stub://three");

                _.Channel("stub://two")
                    .ModifyWith<FakeModifier>()
                    .ModifyWith(new FakeModifier2());
            });

            var channelGraph = channels();

            channelGraph["stub://one".ToUri()].Modifiers.Any().ShouldBeFalse();
            channelGraph["stub://three".ToUri()].Modifiers.Any().ShouldBeFalse();

            channelGraph["stub://two".ToUri()].Modifiers.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(FakeModifier), typeof(FakeModifier2));
        }

        [Fact]
        public void set_accepted_content_types()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.SendMessage<Message1>().To("stub://one");
                _.SendMessage<Message2>().To("stub://two");
                _.SendMessage<Message3>().To("stub://three");

                _.Channel("stub://two").AcceptedContentTypes("application/json");
            });

            channels()["stub://two"].AcceptedContentTypes.Single()
                .ShouldBe("application/json");

            channels()["stub://one"].AcceptedContentTypes.Any().ShouldBeFalse();
        }

        [Fact]
        public void set_default_content_type()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.SendMessage<Message1>().To("stub://one");
                _.SendMessage<Message2>().To("stub://two");
                _.SendMessage<Message3>().To("stub://three");

                _.Channel("stub://two").AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/two");

                _.Channel("stub://three")
                    .AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/one");
            });

            channels()["stub://two"].AcceptedContentTypes
                .ShouldHaveTheSameElementsAs("fake/two", "application/json", "fake/one");

            channels()["stub://three"].AcceptedContentTypes.First()
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
