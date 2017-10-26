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

        [Params(1, 10, 25)]
        public int Parallelization { get; set; }

        [Benchmark]
        public Task InvokeMessage()
        {
            if (Parallelization == 1) return _runtime.Bus.Invoke(new UserCreated {Name = Guid.NewGuid().ToString()});

            var tasks = new Task[Parallelization];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = _runtime.Bus.Invoke(new UserCreated {Name = Guid.NewGuid().ToString()});
            }

            return Task.WhenAll(tasks);


        }
    }
}
