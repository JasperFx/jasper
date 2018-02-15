using System;
using Jasper.Messaging.ErrorHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class ExceptionTypeMatchTester
    {
        [Fact]
        public void matches_by_type()
        {
            var match = new ExceptionTypeMatch<NotImplementedException>(null);

            // Hey, it's important that this code actually works
            match.Matches(null, new NotImplementedException()).ShouldBeTrue();
            match.Matches(null, new Exception()).ShouldBeFalse();
            match.Matches(null, new NotSupportedException()).ShouldBeFalse();
            match.Matches(null, new DivideByZeroException()).ShouldBeFalse();
        }


        [Fact]
        public void matches_by_filter()
        {
            var match = new ExceptionTypeMatch<NotImplementedException>(e => e.Message.Contains("Blue"));

            match.Matches(null, new NotImplementedException("Color is Red")).ShouldBeFalse();
            match.Matches(null, new NotImplementedException("Color is Blue")).ShouldBeTrue();
        }
    }
}
