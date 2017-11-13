using System;
using System.Net.Http;
using Consul;
using Jasper.Util;

namespace Jasper.Consul
{
    /// <summary>
    /// Instructions for how Jasper.Consul builds its ConsulClient
    /// </summary>
    public class ConsulSettings : IDisposable
    {
        private readonly Lazy<ConsulClient> _client;

        public ConsulSettings()
        {
            Port = 8500;


            _client = new Lazy<ConsulClient>(() => new ConsulClient(Configure, ClientOverride, HandlerOverride));
        }

        /// <summary>
        /// Applies alterations to the HttpClient used to connect to Consul
        /// </summary>
        public Action<HttpClient> ClientOverride { get; set; } = x => { };

        /// <summary>
        /// Configures the HttpClientHandler used to connect to Consul
        /// </summary>
        public Action<HttpClientHandler> HandlerOverride { get; set; } = x => { };

        /// <summary>
        /// Configures the ConsulClientConfiguration for the ConsulClient that
        /// Jasper.Consul will use to communicate with Consul
        /// </summary>
        public Action<ConsulClientConfiguration> Configure { get; set; } = x => { };

        /// <summary>
        /// Shorthand way of using all the default configuration for ConsulDotNet,
        /// but overriding just the port number
        /// </summary>
        public int Port
        {
            set
            {
                Configure = _ => _.Address = $"http://localhost:{value}".ToUri();
            }
        }

        public ConsulClient Client => _client.Value;

        void IDisposable.Dispose()
        {
            if (_client.IsValueCreated)
            {
                _client.Value.Dispose();
            }
        }
    }
}
