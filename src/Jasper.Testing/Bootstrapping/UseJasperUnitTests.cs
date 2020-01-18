using System;
using TestingSupport;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.Testing.Bootstrapping
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
