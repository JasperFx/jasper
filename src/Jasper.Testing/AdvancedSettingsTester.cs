using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class AdvancedSettingsTester
    {
        [Fact]
        public void storage_provisioning_is_none_by_default()
        {
            new AdvancedSettings().StorageProvisioning.ShouldBe(StorageProvisioning.None);
        }
    }
}
