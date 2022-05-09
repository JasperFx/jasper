using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BenchmarkDotNet.Attributes;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.SqlServer;

namespace Benchmarks
{
    [MemoryDiagnoser()]
    public class LocalRunner : IDisposable
    {
        private readonly Driver theDriver;

        public LocalRunner()
        {
            theDriver = new Driver();
        }

        public void Dispose()
        {
            theDriver.SafeDispose();
        }

        [Params("SqlServer", "Postgresql", "None")] public string DatabaseEngine;

        [Params(1, 5, 10)] public int NumberOfThreads;

        [IterationSetup]
        public void BuildDatabase()
        {
            theDriver.Start(opts =>
            {
                opts.Advanced.DurabilityAgentEnabled = false;
                switch (DatabaseEngine)
                {
                    case "SqlServer":
                        opts.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
                        break;

                    case "Postgresql":
                        opts.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString);
                        break;
                }

                opts.DefaultLocalQueue
                    .MaximumThreads(NumberOfThreads);

            }).GetAwaiter().GetResult();

        }

        [IterationCleanup]
        public void Teardown()
        {
            theDriver.Teardown().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task Enqueue()
        {
            foreach (var target in theDriver.Targets)
            {
                await theDriver.Publisher.EnqueueAsync(target);
            }

            await theDriver.WaitForAllEnvelopesToBeProcessed();
        }

        [Benchmark]
        public async Task EnqueueMultiThreaded()
        {
            var task1 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Take(200))
                {
                    await theDriver.Publisher.EnqueueAsync(target);
                }
            });

            var task2 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(200).Take(200))
                {
                    await theDriver.Publisher.EnqueueAsync(target);
                }
            });

            var task3 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(400).Take(200))
                {
                    await theDriver.Publisher.EnqueueAsync(target);
                }
            });

            var task4 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(600).Take(200))
                {
                    await theDriver.Publisher.EnqueueAsync(target);
                }
            });

            var task5 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(800))
                {
                    await theDriver.Publisher.EnqueueAsync(target);
                }
            });



            await theDriver.WaitForAllEnvelopesToBeProcessed();
        }



    }
}
