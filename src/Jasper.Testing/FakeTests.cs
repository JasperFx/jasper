using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class FakeTests
    {
        [Fact]
        public void two_plus_two()
        {
            (2+2).ShouldBe(4);
        }
    }
}
