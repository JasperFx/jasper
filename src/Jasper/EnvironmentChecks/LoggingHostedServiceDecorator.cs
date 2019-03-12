using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Jasper.EnvironmentChecks
{
    public class LoggingHostedServiceDecorator : IHostedService
    {
        private readonly IEnvironmentRecorder _recorder;

        public LoggingHostedServiceDecorator(IHostedService inner, IEnvironmentRecorder recorder)
        {
            Inner = inner;
            _recorder = recorder;
        }

        public IHostedService Inner { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Inner.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failure in {Inner}");
                _recorder.Failure($"Failure while running {Inner}.{nameof(IHostedService.StartAsync)}()", e);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Inner.StopAsync(cancellationToken);
        }

        public override string ToString()
        {
            return Inner.ToString();
        }
    }
}
