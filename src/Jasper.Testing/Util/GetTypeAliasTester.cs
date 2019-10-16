﻿using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Util
{
    public class GetTypeAliasTester
    {
        [Fact]
        public void respect_the_type_alias_attribute()
        {
            typeof(AliasedMessage).ToMessageTypeName()
                .ShouldBe("MyThing");
        }

        [Fact]
        public void use_the_types_full_name_otherwise()
        {
            typeof(MySpecialMessage).ToMessageTypeName()
                .ShouldBe(typeof(MySpecialMessage).FullName);
        }

        [Fact]
        public void use_the_version_if_it_exists()
        {
            typeof(AliasedMessage2).ToMessageTypeName()
                .ShouldBe("MyThing.V2");
        }
    }

    [MessageIdentity("MyThing")]
    public class AliasedMessage
    {
    }

    [MessageIdentity("MyThing", Version = 2)]
    public class AliasedMessage2
    {
    }

    public class MySpecialMessage
    {
    }
}
