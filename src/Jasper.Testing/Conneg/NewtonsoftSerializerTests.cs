using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;
using Jasper.Conneg.Json;
using Jasper.Util;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class NewtonsoftSerializerTests
    {
        private readonly BusSettings theSettings = new BusSettings();

        private NewtonsoftSerializerFactory theSerializerFactory
        {
            get
            {
                return new NewtonsoftSerializerFactory(theSettings.JsonSerialization, new DefaultObjectPoolProvider());
            }
        }

        [Fact]
        public void only_json_for_unversioned_message()
        {
            theSettings.MediaSelectionMode = MediaSelectionMode.All;

            theSerializerFactory.ReadersFor(typeof(NonVersionedMessage), MediaSelectionMode.All)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json");

            theSerializerFactory.WritersFor(typeof(NonVersionedMessage), MediaSelectionMode.All)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json");
        }

        [Fact]
        public void json_and_versioned_content_type_with_attribute()
        {
            theSettings.MediaSelectionMode = MediaSelectionMode.All;

            theSerializerFactory.ReadersFor(typeof(VersionedMessage), MediaSelectionMode.All)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json", typeof(VersionedMessage).ToContentType("json"));

            theSerializerFactory.WritersFor(typeof(VersionedMessage), MediaSelectionMode.All)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs("application/json", typeof(VersionedMessage).ToContentType("json"));
        }

        [Fact]
        public void derived_version_for_unversioned_message_when_disallowing_non_typed_serialization()
        {

            theSerializerFactory.ReadersFor(typeof(NonVersionedMessage), MediaSelectionMode.VersionedOnly)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(NonVersionedMessage).ToContentType("json"));

            theSerializerFactory.WritersFor(typeof(NonVersionedMessage), MediaSelectionMode.VersionedOnly)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(NonVersionedMessage).ToContentType("json"));
        }

        [Fact]
        public void derived_version_for_versioned_message_when_disallowing_non_typed_serialization()
        {
            theSettings.MediaSelectionMode = MediaSelectionMode.VersionedOnly;

            theSerializerFactory.ReadersFor(typeof(VersionedMessage), MediaSelectionMode.VersionedOnly)
                .Select(x => x.ContentType).ShouldHaveTheSameElementsAs(typeof(VersionedMessage).ToContentType("json"));

            theSerializerFactory.WritersFor(typeof(VersionedMessage), MediaSelectionMode.VersionedOnly)
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
