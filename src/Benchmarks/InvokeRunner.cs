using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [MemoryDiagnoser()]
    public class InvokeRunner : IDisposable
    {
        private readonly Driver theDriver;

        public InvokeRunner()
        {
            theDriver = new Driver();
        }

        [IterationSetup]
        public void BuildDatabase()
        {
            theDriver.Start(opts =>
            {
                opts.Advanced.DurabilityAgentEnabled = false;


            }).GetAwaiter().GetResult();

        }

        public void Dispose()
        {
            theDriver.SafeDispose();
        }

        [Benchmark()]
        public async Task Invoke()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (var target in theDriver.Targets)
                {
                    await theDriver.Publisher.Invoke(target);
                }
            }
        }


        [Benchmark]
        public async Task InvokeMultiThreaded()
        {
            var task1 = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    foreach (var target in theDriver.Targets.Take(200))
                    {
                        await theDriver.Publisher.Invoke(target);
                    }
                }
            });

            var task2 = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    foreach (var target in theDriver.Targets.Skip(200).Take(200))
                    {
                        await theDriver.Publisher.Invoke(target);
                    }
                }
            });

            var task3 = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    foreach (var target in theDriver.Targets.Skip(400).Take(200))
                    {
                        await theDriver.Publisher.Invoke(target);
                    }
                }
            });

            var task4 = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    foreach (var target in theDriver.Targets.Skip(600).Take(200))
                    {
                        await theDriver.Publisher.Invoke(target);
                    }
                }
            });

            var task5 = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    foreach (var target in theDriver.Targets.Skip(800))
                    {
                        await theDriver.Publisher.Invoke(target);
                    }
                }
            });



            await Task.WhenAll(task1, task2, task3, task4, task5);
        }

    }
}
