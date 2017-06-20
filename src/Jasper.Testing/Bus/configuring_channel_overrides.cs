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
                _.ListenForMessagesFrom("memory://one");
                _.ListenForMessagesFrom("memory://two");
                _.ListenForMessagesFrom("memory://three");

                _.Channel("memory://two").UseAsControlChannel();
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
                _.ListenForMessagesFrom("memory://one");
                _.ListenForMessagesFrom("memory://two")
                    .DeliveryFastWithoutGuarantee();
                _.ListenForMessagesFrom("memory://three");

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
                _.SendMessage<Message1>().To("memory://one");
                _.SendMessage<Message2>().To("memory://two");
                _.SendMessage<Message3>().To("memory://three");

                _.Channel("memory://two")
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

                _.SendMessage<Message1>().To("memory://one");
                _.SendMessage<Message2>().To("memory://two");
                _.SendMessage<Message3>().To("memory://three");

                _.Channel("memory://two").AcceptedContentTypes("application/json");
            });

            channels()["memory://two"].AcceptedContentTypes.Single()
                .ShouldBe("application/json");

            channels()["memory://one"].AcceptedContentTypes.Any().ShouldBeFalse();
        }

        [Fact]
        public void set_default_content_type()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>().Add<Serializer2>();

                _.SendMessage<Message1>().To("memory://one");
                _.SendMessage<Message2>().To("memory://two");
                _.SendMessage<Message3>().To("memory://three");

                _.Channel("memory://two").AcceptedContentTypes("application/json", "fake/one")
                    .DefaultContentType("fake/two");

                _.Channel("memory://three")
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
