using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Baseline;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;

namespace benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {

            BenchmarkRunner.Run<InvokeBenchmark>();
            BenchmarkRunner.Run<HttpPipelineBenchmark>();
            //BenchmarkRunner.Run<PingPongBenchmark>();
        }
    }
}
