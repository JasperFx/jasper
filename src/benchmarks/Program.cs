using BenchmarkDotNet.Running;

namespace benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<InvokeBenchmark>();
            BenchmarkRunner.Run<HttpPipelineBenchmark>();
            //BenchmarkRunner.Run<PingPongBenchmark>();
        }
    }
}
