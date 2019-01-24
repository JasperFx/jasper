using System;
using System.IO;
using System.Threading.Tasks;
using Alba;
using Baseline;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;
using Jasper.TestSupport.Alba;
using TestMessages;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)]
    [MemoryDiagnoser]
    public class HttpPipelineBenchmark : IDisposable
    {
        private readonly string _json;
        private readonly SystemUnderTest _host;

        public HttpPipelineBenchmark()
        {
            _host = JasperAlba.For<Receiver1>();

            var directory = AppContext.BaseDirectory;
            while (!File.Exists(directory.AppendPath("target.json"))) directory = directory.ParentDirectory();

            _json = new FileSystem().ReadStringFromFile(directory.AppendPath("target.json"));
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        [Benchmark]
        public Task RunRequest()
        {
            return _host.Scenario(_ =>
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
