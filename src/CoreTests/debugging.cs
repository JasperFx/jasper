using Jasper;
using Jasper.Messaging;
using Xunit;

namespace CoreTests
{
    public class debugging
    {
        [Fact]
        public void pull_out_root()
        {
            var host = JasperHost.Basic();
            var root = host.Get<IMessagingRoot>();
        }
    }
}
