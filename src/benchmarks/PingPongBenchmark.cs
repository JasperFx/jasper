using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)]
    [MemoryDiagnoser]
    public class PingPongBenchmark : IDisposable
    {
        private readonly JasperRuntime _receiving;
        private readonly JasperRuntime _sender;

        public PingPongBenchmark()
        {
            _receiving = JasperRuntime.For<Receiver1>();
            _sender = JasperRuntime.For<Sender1>();
        }


        [Params(1)] public int Parallelization { get; set; }

        public void Dispose()
        {
            _receiving.Dispose();
            _sender.Dispose();
        }
    }
}
