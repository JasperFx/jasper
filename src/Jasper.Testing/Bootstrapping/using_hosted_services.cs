using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class using_hosted_services
    {
        [Fact]
        public void hosted_service_is_started_and_stopped_in_idiomatic_mode()
        {
            var service = new MyHostedService();

            using (var runtime = JasperHost.For(x => x.Services.AddSingleton<IHostedService>(service)))
            {
                service.WasStarted.ShouldBeTrue();
                service.WasStopped.ShouldBeFalse();
            }

            service.WasStopped.ShouldBeTrue();
        }
    }

    public class MyHostedService : IHostedService
    {
        public bool WasStarted { get; set; }

        public bool WasStopped { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }
    }
}
