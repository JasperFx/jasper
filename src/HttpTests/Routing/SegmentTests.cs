using JasperHttp.Routing;
using Shouldly;
using Xunit;

namespace HttpTests.Routing
{
    public class SegmentTests
    {
        [Fact]
        public void canonical_path_is_just_the_segment()
        {
            new Segment("foo", 2).CanonicalPath().ShouldBe("foo");
        }
    }
}
