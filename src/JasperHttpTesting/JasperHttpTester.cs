using System;
using Alba;
using Jasper;
using Jasper.Http;
using Microsoft.AspNetCore.Hosting;

namespace JasperHttpTesting
{
    public class JasperHttpTester
    {
        public static JasperHttpTester<T> For<T>() where T : JasperHttpRegistry, new()
        {
            return new JasperHttpTester<T>();
        }
    }

    public class JasperHttpTester<T> : SystemUnderTestBase, IDisposable where T : JasperHttpRegistry, new()
    {
        private JasperRuntime _runtime;

        public JasperHttpTester()
        {
            Registry = new T();

        }

        public T Registry { get; }

        protected override IWebHost buildHost()
        {
            _runtime = JasperRuntime.For(Registry);

            Urls = _runtime.Get<JasperUrlLookup>();

            return _runtime.Get<IWebHost>();
        }

        public new void Dispose()
        {
            _runtime.Dispose();
            base.Dispose();
        }

        public override IUrlLookup Urls { get; set; }
    }
}
