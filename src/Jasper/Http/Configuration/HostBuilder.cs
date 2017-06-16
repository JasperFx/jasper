using System;
using Jasper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace Jasper.Http.Configuration
{
    internal class HostBuilder : IWebHostBuilder
    {
        private readonly WebHostBuilder _inner;
        private readonly ServiceRegistry _services;


        public HostBuilder()
        {
            _services = new ServiceRegistry();
            _inner = new WebHostBuilder();
            _inner.ConfigureServices(_ =>
            {
                _.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            });
        }

        public IWebHost Build()
        {
            throw new NotSupportedException("Jasper needs to do the web host building within its bootstrapping");
        }

        public IWebHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            return _inner.UseLoggerFactory(loggerFactory);
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        public IWebHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            return _inner.ConfigureLogging(configureLogging);
        }

        public IWebHostBuilder UseSetting(string key, string value)
        {
            return _inner.UseSetting(key, value);
        }

        public string GetSetting(string key)
        {
            return _inner.GetSetting(key);
        }

        internal IWebHost Activate(IContainer container)
        {
            _inner.ConfigureServices(services => JasperStartup.Register(container, services));
            
            var host = _inner.Build();

            // Annoying, but you do need to do it in this order
            container.Configure(_ => _.AddRegistry(_services));
            return host;
        }
    }
}
