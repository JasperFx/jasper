using System;
using System.IO;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TestMessages;

namespace Benchmarks
{
    public class Driver : IDisposable
    {
        private IHost _host;
        private Task _waiter;

        public Driver()
        {
            var json = File.ReadAllText("targets.json");
            Targets = JsonConvert.DeserializeObject<Target[]>(json);

        }

        public Target[] Targets { get; }

        public async Task Start(Action<JasperOptions> configure)
        {
            _host = await Host.CreateDefaultBuilder()
                .UseJasper(configure)
                .StartAsync();

            Persistence = _host.Services.GetRequiredService<IEnvelopePersistence>();
            Publisher = _host.Services.GetRequiredService<IMessagePublisher>();

            await _host.RebuildMessageStorage();

            _waiter = TargetHandler.WaitForNumber(Targets.Length, 30.Seconds());
        }

        public IMessagePublisher Publisher { get; private set; }

        public Task WaitForAllEnvelopesToBeProcessed()
        {
            return _waiter;
        }

        public T Get<T>()
        {
            return _host.Services.GetRequiredService<T>();
        }

        public IEnvelopePersistence Persistence { get; private set; }

        public async Task Teardown()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host = null;
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
