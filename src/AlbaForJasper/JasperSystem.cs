using System;
using System.Threading.Tasks;
using Alba;
using Jasper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using StructureMap;

namespace AlbaForJasper
{
    public class JasperSystem
    {
        public static JasperSystem<T> For<T>() where T : JasperRegistry, new()
        {
            return new JasperSystem<T>();
        }
    }

    public static class JasperRuntimeAlbaExtensions
    {
        /// <summary>
        /// Run an Alba scenario test against a Jasper application
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Task<IScenarioResult> Scenario(this JasperRuntime runtime, Action<Scenario> configuration)
        {
            var system = new JasperAlbaUsage(runtime);
            return system.Scenario(configuration);
        }
    }

    public class JasperAlbaUsage : SystemUnderTestBase
    {
        private readonly JasperRuntime _runtime;

        // TODO -- bring in the IHostingEnvironment attached to the runtime
        // When it exists. See https://github.com/JasperFx/jasper/issues/91
        public JasperAlbaUsage(JasperRuntime runtime) : base(null)
        {
            _runtime = runtime;
        }


        protected override IWebHost buildHost()
        {
            return _runtime.Get<IWebHost>();
        }
    }

    public class JasperSystem<T> : SystemUnderTestBase, IDisposable where T : JasperRegistry, new()
    {
        private JasperRuntime _runtime;

        public JasperSystem()
        {
            Registry = new T();
        }

        public T Registry { get; }
        protected override IWebHost buildHost()
        {
            _runtime = JasperRuntime.For(Registry);

            return _runtime.Get<IWebHost>();
        }

        // TODO -- set UrlHelper if it exists

        public new void Dispose()
        {
            _runtime.Dispose();
            base.Dispose();
        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {
            // Nothing
        }
    }
}
