using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Util
{
    public class GetTypeAliasTester
    {
        [Fact]
        public void respect_the_type_alias_attribute()
        {
            typeof(AliasedMessage).ToMessageAlias()
                .ShouldBe("MyThing");
        }

        [Fact]
        public void use_the_types_full_name_otherwise()
        {
            typeof(MySpecialMessage).ToMessageAlias()
                .ShouldBe(typeof(MySpecialMessage).FullName);
        }
    }

    [MessageAlias("MyThing")]
    public class AliasedMessage
    {

    }

    public class MySpecialMessage
    {

    }
}
