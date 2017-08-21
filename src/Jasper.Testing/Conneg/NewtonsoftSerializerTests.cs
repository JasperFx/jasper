using System.Linq;
using Jasper.Bus;
using Jasper.Conneg;
using Jasper.Util;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class NewtonsoftSerializerTests
    {
        private readonly BusSettings theSettings = new BusSettings();
        private NewtonsoftSerializer theSerializer;

        public NewtonsoftSerializerTests()
        {
            theSerializer = new NewtonsoftSerializer(theSettings);
        }

        [Fact]
        public void only_json_for_unversioned_message()
        {
            theSettings.AllowNonVersionedSerialization = true;

            theSerializer.ReadersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json");

            theSerializer.WritersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json");
        }

        [Fact]
        public void json_and_versioned_content_type_with_attribute()
        {
            theSettings.AllowNonVersionedSerialization = true;

            theSerializer.ReadersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json", typeof(VersionedMessage).ToContentType("json"));

            theSerializer.WritersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json", typeof(VersionedMessage).ToContentType("json"));
        }

        [Fact]
        public void derived_version_for_unversioned_message_when_disallowing_non_typed_serialization()
        {
            theSettings.AllowNonVersionedSerialization = false;

            theSerializer.ReadersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(NonVersionedMessage).ToContentType("json"));

            theSerializer.WritersFor(typeof(NonVersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(NonVersionedMessage).ToContentType("json"));
        }

        [Fact]
        public void derived_version_for_versioned_message_when_disallowing_non_typed_serialization()
        {
            theSettings.AllowNonVersionedSerialization = false;

            theSerializer.ReadersFor(typeof(VersionedMessage))
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(VersionedMessage).ToContentType("json"));

            theSerializer.WritersFor(typeof(VersionedMessage))
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
