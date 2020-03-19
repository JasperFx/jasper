using System;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class UseJasperGuardClauseTests
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
