using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class configuring_serialization : BootstrappingContext
    {
        [Fact]
        public void disallow_non_versioned_serialization()
        {
            new BusSettings().MediaSelectionMode.ShouldBe(MediaSelectionMode.All);

            theRegistry.Advanced.MediaSelectionMode = MediaSelectionMode.VersionedOnly;

            theRuntime.Get<BusSettings>().MediaSelectionMode
                .ShouldBe(MediaSelectionMode.VersionedOnly);
        }
    }
}
