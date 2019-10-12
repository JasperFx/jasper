using JasperHttp;
using Shouldly;
using Xunit;

namespace HttpTests
{
    public class HttpSettingsTests
    {
        [Fact]
        public void default_compliance_mode_is_full()
        {
            new JasperHttpOptions().AspNetCoreCompliance.ShouldBe(ComplianceMode.FullyCompliant);
        }
    }
}
