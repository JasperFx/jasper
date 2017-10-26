using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)][MemoryDiagnoser]
    public class PingPongBenchmark : IDisposable
    {
        private readonly JasperRuntime _receiving;
        private readonly JasperRuntime _sender;

        public PingPongBenchmark()
        {
            _receiving = JasperRuntime.For<Receiver1>();
            _sender = JasperRuntime.For<Sender1>();
        }

        public void Dispose()
        {
            _receiving.Dispose();
            _sender.Dispose();
        }

        [Benchmark()]
        public Task RequestReply()
        {
            var ping = new Ping();
            return _sender.Bus.Request<Pong>(ping);
        }

        [Params(1)]
        public int Parallelization { get; set; }

        [Benchmark]
        public Task RequestReplyMultiThreaded()
        {
            if (Parallelization == 1) return _sender.Bus.Request<Pong>(new Ping());

            var tasks = new Task[Parallelization];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = _sender.Bus.Request<Pong>(new Ping());
            }

            return Task.WhenAll(tasks);
        }
    }
}