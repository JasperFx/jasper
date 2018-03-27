using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class using_hosted_services
    {
        [Fact]
        public async Task hosted_service_is_started_and_stopped_in_idiomatic_mode()
        {
            var service = new MyHostedService();

            var runtime = await JasperRuntime.ForAsync(x => x.Services.AddSingleton<IHostedService>(service));

            service.WasStarted.ShouldBeTrue();
            service.WasStopped.ShouldBeFalse();

            await runtime.Shutdown();

            service.WasStopped.ShouldBeTrue();
        }
    }

    public class MyHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public bool WasStarted { get; set; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }

        public bool WasStopped { get; set; }
    }
}
