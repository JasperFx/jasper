using System;
using System.IO;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class configuring_serialization_preferences : IntegrationContext
    {
        [Fact]
        public void json_serialization_is_the_default()
        {
            withAllDefaults();

            Runtime.Container.GetInstance<ChannelGraph>().AcceptedContentTypes.ShouldHaveTheSameElementsAs("application/json");

            Bootstrapping.ContainerExtensions.ShouldHaveRegistration<ISerializer, JsonSerializer>(Runtime.Container);
        }

        [Fact]
        public void add_additional_serializers()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>();
                _.Serialization.Add<Serializer2>();
            });

            Runtime.Container.GetInstance<ChannelGraph>().AcceptedContentTypes
                .ShouldHaveTheSameElementsAs("application/json", "fake/one", "fake/two");

            Bootstrapping.ContainerExtensions.ShouldHaveRegistration<ISerializer, Serializer1>(Runtime.Container);
            Bootstrapping.ContainerExtensions.ShouldHaveRegistration<ISerializer, Serializer2>(Runtime.Container);
        }

        [Fact]
        public void override_the_accepted_content_type_order()
        {
            with(_ =>
            {
                _.Serialization.Add<Serializer1>();
                _.Serialization.Add<Serializer2>();
                _.Serialization.ContentPreferenceOrder("fake/two", "application/json");
            });

            // Back fills anything missing
            Runtime.Container.GetInstance<ChannelGraph>().AcceptedContentTypes
                .ShouldHaveTheSameElementsAs("fake/two", "application/json", "fake/one");
        }

        [Fact]
        public void try_to_prioritize_a_missing_content_type()
        {
            Exception<UnknownContentTypeException>.ShouldBeThrownBy(() =>
            {
                with(_ =>
                {
                    _.Serialization.Add<Serializer1>();
                    _.Serialization.Add<Serializer2>();
                    _.Serialization.ContentPreferenceOrder("fake/nonexistent", "application/json");
                });
            });
        }
    }

    public abstract class FakeSerializer : ISerializer
    {
        public FakeSerializer(string contentType)
        {
            ContentType = contentType;
        }

        public void Serialize(object message, Stream stream)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(Stream message)
        {
            throw new NotImplementedException();
        }

        public string ContentType { get; }
    }

    public class Serializer1 : FakeSerializer
    {
        public Serializer1() : base("fake/one")
        {
        }
    }

    public class Serializer2 : FakeSerializer
    {
        public Serializer2() : base("fake/two")
        {
        }
    }


}