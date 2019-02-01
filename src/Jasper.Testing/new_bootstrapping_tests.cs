using System;
using Baseline;
using Jasper.Http;
using Lamar;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class new_bootstrapping_tests : IDisposable
    {
        private IWebHost theHost;

        public new_bootstrapping_tests()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<EmptyStartup>()
                .UseJasper();

            theHost = builder.Build();
        }

        public void Dispose()
        {
            theHost?.Dispose();
        }

        [Fact]
        public void does_not_blow_up()
        {
            theHost.ShouldNotBeNull();
        }

        [Fact]
        public void the_container_is_lamar()
        {
            theHost.Services.ShouldBeOfType<Container>();
        }
    }
}
