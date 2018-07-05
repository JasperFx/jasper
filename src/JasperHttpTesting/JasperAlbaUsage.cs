using System;
using System.Text;
using System.Threading.Tasks;
using Alba;
using Alba.Stubs;
using Jasper;
using Jasper.Http;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace JasperHttpTesting
{
    public class JasperAlbaUsage : ISystemUnderTest
    {
        protected readonly JasperRuntime _runtime;

        public JasperAlbaUsage(JasperRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Urls = new JasperUrlLookup(_runtime.Get<IUrlRegistry>());
            Features = _runtime.Get<IServer>().Features;
            Services = _runtime.Container;
            Invoker = _runtime.RequestDelegate;


        }

        public void Dispose()
        {

        }

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public virtual Task BeforeEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public T FromJson<T>(string json)
        {
            var graph = _runtime.Get<HttpSerializationGraph>();
            return (T) graph.JsonReaderFor(typeof(T)).ReadFromData(Encoding.Default.GetBytes(json));
        }

        public string ToJson(object target)
        {
            var graph = _runtime.Get<HttpSerializationGraph>();
            var bytes = graph.JsonWriterFor(target.GetType()).Write(target);

            return Encoding.Default.GetString(bytes);
        }

        public IUrlLookup Urls { get; set; }
        public IFeatureCollection Features { get; }
        public IServiceProvider Services { get; }
        public RequestDelegate Invoker { get; }
    }
}
