using System;
using Jasper.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class OptionsExtensionsTests
    {
        [Theory]
        [InlineData(typeof(JasperOptions), "Jasper")]
        [InlineData(typeof(FakeSettings), "Fake")]
        [InlineData(typeof(OptionsExtensionsTests), "OptionsExtensionsTests")]
        public void get_section_name(Type type, string sectionName)
        {
            type.ConfigSectionName().ShouldBe(sectionName);
        }

        public class FakeSettings
        {
            public int SomeSetting { get; set; }
        }
    }
}
