using System;
using Jasper;
using Microsoft.AspNetCore.Hosting;
using TestingSupport;
using Xunit;

namespace CoreTests.Bootstrapping
{
    public class UseJasperUnitTests
    {
        [Fact]
        public void cannot_call_use_jasper_twice()
        {
            var builder = JasperHost.CreateDefaultBuilder();

            builder.UseJasper();

            Exception<InvalidOperationException>.ShouldBeThrownBy(() => builder.UseJasper());
        }
    }
}
