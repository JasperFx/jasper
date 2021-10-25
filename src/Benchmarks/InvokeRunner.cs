using System;
using System.Threading.Tasks;
using Baseline;
using BenchmarkDotNet.Attributes;
using IntegrationTests;

namespace Benchmarks
{
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

        [Benchmark]
        public async Task Invoke()
        {
            foreach (var target in theDriver.Targets)
            {
                await theDriver.Publisher.Invoke(target);
            }
        }
    }
}
