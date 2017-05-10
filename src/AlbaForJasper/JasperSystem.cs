using System;
using Alba;
using Jasper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
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

            return _runtime.Container.GetInstance<IWebHost>();
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