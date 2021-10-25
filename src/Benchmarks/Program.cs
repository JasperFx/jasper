using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Jasper;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
 using TestMessages;

namespace Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PersistenceRunner>();

            // using var host = JasperHost.For(opts =>
            // {
            //void
            // });
            //
            // var waiter = TargetHandler.WaitForNumber(5, 10.Seconds());
            //
            // var publisher = host.Services.GetRequiredService<ICommandBus>();
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            //
            // await waiter;


        }
    }

    public static class RabbitTesting
    {
        public static int Number = 0;

        public static string NextQueueName() => $"perf{++Number}";
        public static string NextExchangeName() => $"perf{++Number}";
    }

    public class Driver : IDisposable
    {
        private IHost _host;

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

            await _host.RebuildMessageStorage();
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
