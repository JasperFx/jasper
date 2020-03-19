using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class environment_sensitive_configuration
    {
        [Fact]
        public void bootstrap_with_registry_that_has_configuration()
        {
            using (var host = Host.CreateDefaultBuilder().UseJasper<EnvironmentalJasperApp>()
                .UseEnvironment("Development").Build())
            {
                host.Services.GetRequiredService<RegisteredMarker>()
                    .Name.ShouldBe("Tanoh Kpassognon");
            }

            using (var host = Host.CreateDefaultBuilder().UseJasper<EnvironmentalJasperApp>()
                .UseEnvironment("Production").Build())
            {
                host.Services.GetRequiredService<RegisteredMarker>()
                    .Name.ShouldBe("Emmanuel Ogbah");
            }
        }

        [Fact]
        public void can_use_hosting_as_part_of_the_configuration()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseJasper((context, jasper) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        jasper.Services.AddSingleton(new RegisteredMarker {Name = "Kendall Fuller"});
                    }

                    if (context.HostingEnvironment.IsStaging())
                    {
                        jasper.Services.AddSingleton(new RegisteredMarker {Name = "Darrel Williams"});
                    }
                })
                .UseEnvironment("Development");

            using (var host = builder.Build())
            {
                host.Services.GetRequiredService<RegisteredMarker>()
                    .Name.ShouldBe("Kendall Fuller");
            }
        }
    }

    public class EnvironmentalJasperApp : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            if (hosting.IsDevelopment())
            {
                Services.AddSingleton(new RegisteredMarker {Name = "Tanoh Kpassognon"});
            }

            if (hosting.IsProduction())
            {
                Services.AddSingleton(new RegisteredMarker {Name = "Emmanuel Ogbah"});
            }
        }
    }

    public class RegisteredMarker
    {
        public string Name { get; set; }
    }
}
