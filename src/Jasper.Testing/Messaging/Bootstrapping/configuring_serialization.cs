using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Messaging.Transports.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class configuring_serialization : BootstrappingContext
    {
        [Fact]
        public async Task disallow_non_versioned_serialization()
        {
            new MessagingSettings().MediaSelectionMode.ShouldBe(MediaSelectionMode.All);

            theRegistry.Advanced.MediaSelectionMode = MediaSelectionMode.VersionedOnly;

            var runtime = await theRuntime();

            runtime.Get<MessagingSettings>().MediaSelectionMode
                .ShouldBe(MediaSelectionMode.VersionedOnly);
        }
    }
}
