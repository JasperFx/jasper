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
        private readonly IJasperHost _receiving;
        private readonly IJasperHost _sender;

        public PingPongBenchmark()
        {
            _receiving = JasperHost.For<Receiver1>();
            _sender = JasperHost.For<Sender1>();
        }


        [Params(1)] public int Parallelization { get; set; }

        public void Dispose()
        {
            _receiving.Dispose();
            _sender.Dispose();
        }
    }
}
