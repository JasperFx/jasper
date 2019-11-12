using Jasper.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class JasperOptionsTests
    {
        private readonly JasperOptions theSettings = new JasperOptions();

        [Fact]
        public void unique_node_id_is_really_unique()
        {
            var options1 = new JasperOptions();
            var options2 = new JasperOptions();
            var options3 = new JasperOptions();
            var options4 = new JasperOptions();
            var options5 = new JasperOptions();
            var options6 = new JasperOptions();

            options1.UniqueNodeId.ShouldNotBe(options2.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options3.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options2.UniqueNodeId.ShouldNotBe(options3.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options3.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options3.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options3.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options4.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options4.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options5.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);
        }
    }
}
