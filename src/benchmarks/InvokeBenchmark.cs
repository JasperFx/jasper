using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)][MemoryDiagnoser]
    public class InvokeBenchmark : IDisposable
    {
        private readonly JasperRuntime _runtime;

        public InvokeBenchmark()
        {
            _runtime = JasperRuntime.For<Sender1>();
        }

        public void Dispose()
        {
            _runtime.Dispose();
        }

        [Benchmark]
        public Task InvokeMessage()
        {
            return _runtime.Bus.Invoke(new UserCreated {Name = Guid.NewGuid().ToString()});
        }
    }
}