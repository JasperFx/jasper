using Jasper.Runtime;
using Xunit;

namespace Jasper.Testing
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
