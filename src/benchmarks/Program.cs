using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace benchmarks
{
    internal class DotNetCore21Config : ManualConfig
    {
        public DotNetCore21Config()
        {
            Add(Job.Default.With(CsProjCoreToolchain.NetCoreApp21));
            Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
            Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            //BenchmarkRunner.Run<RoutingBenchmark>();
            BenchmarkRunner.Run<FastModeRoutingBenchmark>(new DotNetCore21Config());
            //BenchmarkRunner.Run<InvokeBenchmark>();
            //BenchmarkRunner.Run<HttpPipelineBenchmark>();
            //BenchmarkRunner.Run<PingPongBenchmark>();
        }
    }


}
