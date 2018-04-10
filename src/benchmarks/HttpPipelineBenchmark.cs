using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;
using JasperHttpTesting;
using TestMessages;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)][MemoryDiagnoser]
    public class HttpPipelineBenchmark : IDisposable
    {
        private JasperRuntime _runtime;
        private string _json;

        public HttpPipelineBenchmark()
        {
            _runtime = JasperRuntime.For<Receiver1>();

            var directory = AppContext.BaseDirectory;
            while (!File.Exists(directory.AppendPath("target.json")))
            {
                directory = directory.ParentDirectory();
            }

            _json = new FileSystem().ReadStringFromFile(directory.AppendPath("target.json"));

        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        [Benchmark]
        public Task RunRequest()
        {
            return _runtime.Scenario(_ =>
            {
                _.Post.Url("/target");
                _.Body.JsonInputIs(_json);
            });
        }
    }

    public class TargetEndpoint
    {
        public void post_target(Target target)
        {

        }
    }


}
