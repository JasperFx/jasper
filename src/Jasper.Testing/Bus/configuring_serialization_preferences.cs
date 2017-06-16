using System;
using System.IO;
using JasperBus.Configuration;
using JasperBus.Runtime.Serializers;
using JasperBus.Tests.Bootstrapping;
using Shouldly;
using Xunit;

namespace JasperBus.Tests
{
    public class configuring_serialization_preferences : IntegrationContext
    {
        [Fact]
        public void json_serialization_is_the_default()
        {
            withAllDefaults();

            Runtime.Container.GetInstance<ChannelGraph>().AcceptedContentTypes.ShouldHaveTheSameElementsAs("application/json");

            Runtime.Container.ShouldHaveRegistration<IMessageSerializer, JsonMessageSerializer>();
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

            Runtime.Container.ShouldHaveRegistration<IMessageSerializer, Serializer1>();
            Runtime.Container.ShouldHaveRegistration<IMessageSerializer, Serializer2>();
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

    public abstract class FakeMessageSerializer : IMessageSerializer
    {
        public FakeMessageSerializer(string contentType)
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

    public class Serializer1 : FakeMessageSerializer
    {
        public Serializer1() : base("fake/one")
        {
        }
    }

    public class Serializer2 : FakeMessageSerializer
    {
        public Serializer2() : base("fake/two")
        {
        }
    }


}