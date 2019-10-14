using System;
using Jasper;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using Xunit;

namespace CoreTests.Bootstrapping
{
    public class UseJasperUnitTests
    {
        [Fact]
        public void cannot_call_use_jasper_twice()
        {
            var builder = Host.CreateDefaultBuilder();

            builder.UseJasper();

            Exception<InvalidOperationException>.ShouldBeThrownBy(() => builder.UseJasper());
        }
    }
}
