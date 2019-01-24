using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)]
    [MemoryDiagnoser]
    public class InvokeBenchmark : IDisposable
    {
        private readonly IJasperHost _host;

        public InvokeBenchmark()
        {
            _host = JasperHost.For<Sender1>();
        }

        [Params(1, 10, 25)] public int Parallelization { get; set; }

        public void Dispose()
        {
            _host.Dispose();
        }

        [Benchmark]
        public Task InvokeMessage()
        {
            if (Parallelization == 1)
                return _host.Messaging.Invoke(new UserCreated
                {
                    Name = Guid.NewGuid().ToString()
                });

            var tasks = new Task[Parallelization];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = _host.Messaging.Invoke(new UserCreated {Name = Guid.NewGuid().ToString()});

            return Task.WhenAll(tasks);
        }
    }
}
