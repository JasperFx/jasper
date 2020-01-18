using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

#if NETSTANDARD2_0
using Host = Microsoft.AspNetCore.WebHost;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
#else
using Host = Microsoft.Extensions.Hosting.Host;
#endif

namespace TestingSupport
{
    public abstract class BaseContext : IDisposable
    {
        private readonly bool _shouldStart;
        protected readonly IHostBuilder builder = Host.CreateDefaultBuilder();


        private IHost _host;

        protected BaseContext(bool shouldStart)
        {
            _shouldStart = shouldStart;
        }

        public IHost theHost
        {
            get
            {
                if (_host == null)
                {
                    _host = builder.Build();
                    if (_shouldStart)
                    {
                        _host.Start();
                    }
                }

                return _host;
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
