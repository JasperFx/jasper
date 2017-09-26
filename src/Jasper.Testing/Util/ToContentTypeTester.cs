using BlueMilk.Scanning;
using Jasper.Bus;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Util
{
    public class ToContentTypeTester
    {
        [Fact]
        public void with_no_attribute()
        {
            typeof(MyUnversionedMessage).ToVersion()
                .ShouldBe("V1");
        }

        [Fact]
        public void with_the_attribute()
        {
            typeof(MyMessage).ToVersion()
                .ShouldBe("V2");
        }

        [Fact]
        public void to_content_type_with_version_attribute()
        {
            typeof(MyMessage).ToContentType("json")
                .ShouldBe($"application/vnd.{typeof(MyMessage).ToMessageAlias().ToLower()}.v2+json");
        }

        [Fact]
        public void to_content_type_with_no_version_attribute()
        {
            typeof(MyMessage).ToContentType("json")
                .ShouldBe($"application/vnd.mymessage.v2+json");
        }
    }

    public class MyUnversionedMessage
    {

    }

    [Version("V2"), MessageAlias("MyMessage")]
    public class MyMessage
    {

    }


}
