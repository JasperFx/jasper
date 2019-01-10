using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<RoutingBenchmark>();
            //BenchmarkRunner.Run<InvokeBenchmark>();
            //BenchmarkRunner.Run<HttpPipelineBenchmark>();
            //BenchmarkRunner.Run<PingPongBenchmark>();
        }
    }


}
