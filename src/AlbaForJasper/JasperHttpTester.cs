using System;
using System.Linq.Expressions;
using Alba;
using Jasper;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using StructureMap;

namespace AlbaForJasper
{
    public class JasperHttpTester
    {
        public static JasperHttpTester<T> For<T>() where T : JasperRegistry, new()
        {
            return new JasperHttpTester<T>();
        }
    }

    public class JasperHttpTester<T> : SystemUnderTestBase, IDisposable where T : JasperRegistry, new()
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

    public class JasperUrlLookup : IUrlLookup
    {
        private readonly IUrlRegistry _urls;

        public JasperUrlLookup(IUrlRegistry urls)
        {
            _urls = urls;
        }

        public string UrlFor<T>(Expression<Action<T>> expression, string httpMethod)
        {
            return _urls.UrlFor(expression, httpMethod);
        }

        public string UrlFor<T>(string method)
        {
            return _urls.UrlFor<T>(method);
        }

        public string UrlFor<T>(T input, string httpMethod)
        {
            return _urls.UrlFor(input, httpMethod);
        }
    }
}
