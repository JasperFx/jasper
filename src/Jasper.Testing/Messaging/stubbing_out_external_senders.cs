using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class stubbing_out_external_senders
    {
        [Fact]
        public void stub_out_external_setting_via_IEndpoints()
        {
            var options = new JasperOptions();
            options.Advanced.StubAllOutgoingExternalSenders.ShouldBeFalse();

            options.Endpoints.StubAllExternallyOutgoingEndpoints();

            options.Advanced.StubAllOutgoingExternalSenders.ShouldBeTrue();
        }
    }
}
