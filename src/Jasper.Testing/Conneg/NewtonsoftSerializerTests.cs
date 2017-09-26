using System.Linq;
using BlueMilk.Scanning;
using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;
using Jasper.Util;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class NewtonsoftSerializerTests
    {
        private readonly BusSettings theSettings = new BusSettings();
        private NewtonsoftSerializerFactory theSerializerFactory;

        public NewtonsoftSerializerTests()
        {
            theSerializerFactory = new NewtonsoftSerializerFactory(theSettings);
        }

        [Fact]
        public void only_json_for_unversioned_message()
        {
            theSettings.AllowNonVersionedSerialization = true;

            theSerializerFactory.ReadersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json");

            theSerializerFactory.WritersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json");
        }

        [Fact]
        public void json_and_versioned_content_type_with_attribute()
        {
            theSettings.AllowNonVersionedSerialization = true;

            theSerializerFactory.ReadersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json", typeof(VersionedMessage).ToContentType("json"));

            theSerializerFactory.WritersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json", typeof(VersionedMessage).ToContentType("json"));
        }

        [Fact]
        public void derived_version_for_unversioned_message_when_disallowing_non_typed_serialization()
        {
            theSettings.AllowNonVersionedSerialization = false;

            theSerializerFactory.ReadersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(NonVersionedMessage).ToContentType("json"));

            theSerializerFactory.WritersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(NonVersionedMessage).ToContentType("json"));
        }

        [Fact]
        public void derived_version_for_versioned_message_when_disallowing_non_typed_serialization()
        {
            theSettings.AllowNonVersionedSerialization = false;

            theSerializerFactory.ReadersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(VersionedMessage).ToContentType("json"));

            theSerializerFactory.WritersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(VersionedMessage).ToContentType("json"));
        }
    }

    public class NonVersionedMessage
    {

    }

    [Version("V2")]
    public class VersionedMessage
    {

    }
}
