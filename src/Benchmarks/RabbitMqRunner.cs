using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BenchmarkDotNet.Attributes;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.SqlServer;
using Jasper.RabbitMQ;

namespace Benchmarks
{
    [MemoryDiagnoser()]
    public class RabbitMqRunner : IDisposable
    {
        private readonly Driver theDriver;

        public RabbitMqRunner()
        {
            theDriver = new Driver();
        }

        public void Dispose()
        {
            theDriver.SafeDispose();
        }

        [Params("Postgresql", "None")] public string DatabaseEngine;

        [Params(1, 5, 10)] public int NumberOfThreads;
        [Params(1, 3, 5)] public int ListenerCount;

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

                var queueName = RabbitTesting.NextQueueName();

                opts.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue(queueName);
                    x.AutoProvision = true;
                });

                opts.PublishAllMessages().ToRabbit(queueName);
                opts.ListenToRabbitQueue(queueName)
                    .MaximumThreads(NumberOfThreads)
                    .ListenerCount(ListenerCount);

            }).GetAwaiter().GetResult();

            theDriver.Host.TryPurgeAllRabbitMqQueues();

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
                await theDriver.Publisher.Enqueue(target);
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
                    await theDriver.Publisher.Enqueue(target);
                }
            });

            var task2 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(200).Take(200))
                {
                    await theDriver.Publisher.Enqueue(target);
                }
            });

            var task3 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(400).Take(200))
                {
                    await theDriver.Publisher.Enqueue(target);
                }
            });

            var task4 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(600).Take(200))
                {
                    await theDriver.Publisher.Enqueue(target);
                }
            });

            var task5 = Task.Factory.StartNew(async () =>
            {
                foreach (var target in theDriver.Targets.Skip(800))
                {
                    await theDriver.Publisher.Enqueue(target);
                }
            });



            await theDriver.WaitForAllEnvelopesToBeProcessed();
        }



    }
}
