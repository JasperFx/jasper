using Jasper.Conneg;
using Jasper.Messaging.Transports.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class configuring_serialization : BootstrappingContext
    {
        [Fact]
        public void disallow_non_versioned_serialization()
        {
            new MessagingSettings().MediaSelectionMode.ShouldBe(MediaSelectionMode.All);

            theRegistry.Advanced.MediaSelectionMode = MediaSelectionMode.VersionedOnly;

            theRuntime.Get<MessagingSettings>().MediaSelectionMode
                .ShouldBe(MediaSelectionMode.VersionedOnly);
        }
    }
}
