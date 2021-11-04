using System;
using System.IO;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TestMessages;

namespace Benchmarks
{
    public class Driver : IDisposable
    {
        private Task _waiter;

        public Driver()
        {
            var json = File.ReadAllText("targets.json");
            Targets = JsonConvert.DeserializeObject<Target[]>(json);

        }

        public Target[] Targets { get; }

        public async Task Start(Action<JasperOptions> configure)
        {

            Host = await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    configure(opts);
                    opts.Advanced.CodeGeneration.ApplicationAssembly = GetType().Assembly;
                    opts.Advanced.CodeGeneration.TypeLoadMode = TypeLoadMode.LoadFromPreBuiltAssembly;
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .StartAsync();

            Persistence = Host.Services.GetRequiredService<IEnvelopePersistence>();
            Publisher = Host.Services.GetRequiredService<IMessagePublisher>();

            await Host.RebuildMessageStorage();

            _waiter = TargetHandler.WaitForNumber(Targets.Length, 60.Seconds());
        }

        public IHost Host { get; private set; }

        public IMessagePublisher Publisher { get; private set; }

        public Task WaitForAllEnvelopesToBeProcessed()
        {
            return _waiter;
        }

        public T Get<T>()
        {
            return Host.Services.GetRequiredService<T>();
        }

        public IEnvelopePersistence Persistence { get; private set; }

        public async Task Teardown()
        {
            if (Host != null)
            {
                await Host.StopAsync();
                Host = null;
            }
        }

        public void Dispose()
        {
            Host?.Dispose();
        }
    }
}
