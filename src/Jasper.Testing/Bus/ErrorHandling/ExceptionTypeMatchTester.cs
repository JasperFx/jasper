using System;
using JasperBus.ErrorHandling;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.ErrorHandling
{
    public class ExceptionTypeMatchTester
    {
        [Fact]
        public void matches_by_type()
        {
            var match = new ExceptionTypeMatch<NotImplementedException>();

            // Hey, it's important that this code actually works
            match.Matches(null, new NotImplementedException()).ShouldBeTrue();
            match.Matches(null, new Exception()).ShouldBeFalse();
            match.Matches(null, new NotSupportedException()).ShouldBeFalse();
            match.Matches(null, new DivideByZeroException()).ShouldBeFalse();
        }
    }
}