using System;
using Jasper;
using Jasper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace JasperHttp.Tests
{
    public class JasperWebHostBuilderExtensionsTester
    {
        private readonly IWebHost theHost;

        public JasperWebHostBuilderExtensionsTester()
        {
            var builder = new WebHostBuilder();
            builder.UseKestrel();
            builder.ConfigureServices(x => x.AddSingleton<IService, Service>());
            builder.AddJasper<BootstrappingApp>();

            theHost = builder.Build();
        }

        [Fact]
        public void is_using_structuremap_for_the_service_provider()
        {
            theHost.Services.ShouldBeOfType<StructureMapServiceProvider>();
        }

        [Fact]
        public void has_service_registrations_from_outside_of_jasper()
        {
            theHost.Services.GetService<IService>()
                .ShouldBeOfType<Service>();
        }

        [Fact]
        public void has_service_registrations_from_jasper()
        {
            theHost.Services.GetService<BootstrappingToken>()
                .Id.ShouldBe(BootstrappingApp.Id);
        }

        [Fact]
        public void jasper_runtime_is_disposed_when_the_host_is_disposed()
        {
            var runtime = theHost.Services.GetService<JasperRuntime>();
            runtime.IsDisposed.ShouldBeFalse();

            theHost.Dispose();

            runtime.IsDisposed.ShouldBeTrue();
        }

        public interface IService { }
        public class Service : IService { }
    }

    

    public class BootstrappingApp : JasperRegistry
    {
        public static readonly Guid Id = Guid.NewGuid();

        public BootstrappingApp()
        {
            Services.For<BootstrappingToken>().Use(new BootstrappingToken(Id));
        }
    }

    public class BootstrappingToken
    {
        public Guid Id { get; }

        public BootstrappingToken(Guid id)
        {
            Id = id;
        }
    }
}
