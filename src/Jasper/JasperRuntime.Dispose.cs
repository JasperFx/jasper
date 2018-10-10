using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Lamar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper
{
    public partial class JasperRuntime
    {
        public void Dispose()
        {
            // Because StackOverflowException's are a drag
            if (IsDisposed || isDisposing) return;

            Shutdown().GetAwaiter().GetResult();
        }

        public async Task Shutdown()
        {
            // Because StackOverflowException's are a drag
            if (IsDisposed || isDisposing) return;

            if (Registry.BootstrappedWithinAspNetCore)
            {
                IsDisposed = true;
                return;
            }

            _lifetime.StopApplication();

            await shutdownAspNetCoreServer();

            if (Settings.HostedServicesEnabled)
            {
                await shutdownHostedServices();
            }

            _lifetime.NotifyStopped();

            isDisposing = true;

            Container.DisposalLock = DisposalLock.Unlocked;
            Container.Dispose();

            IsDisposed = true;
        }

        private async Task shutdownHostedServices()
        {
            foreach (var hostedService in _hostedServices)
            {
                try
                {
                    await hostedService.StopAsync(CancellationToken.None);
                }
                catch (Exception e)
                {
                    Get<ILogger<IHostedService>>().LogError(e, "Failure while stopping " + hostedService);
                }
            }
        }
    }
}
