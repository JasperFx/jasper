using System;
using Jasper.Bus.ErrorHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.ErrorHandling
{
    public class ExceptionTypeMatchTester
    {
        [Fact]
        public void matches_by_type()
        {
            var match = new ExceptionTypeMatch<NotImplementedException>();

            // Hey, it's important that this code actually works
            ShouldBeBooleanExtensions.ShouldBeTrue(match.Matches(null, new NotImplementedException()));
            ShouldBeBooleanExtensions.ShouldBeFalse(match.Matches(null, new Exception()));
            ShouldBeBooleanExtensions.ShouldBeFalse(match.Matches(null, new NotSupportedException()));
            ShouldBeBooleanExtensions.ShouldBeFalse(match.Matches(null, new DivideByZeroException()));
        }
    }
}