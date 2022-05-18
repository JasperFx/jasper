using System;
using Jasper.RabbitMQ.Internal;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;

namespace Jasper.RabbitMQ.Tests.ConventionalRouting
{
    public abstract class ConventionalRoutingContext : IDisposable
    {
        private IHost _host;

        public void Dispose()
        {
            _host?.Dispose();
        }

        internal void ConfigureConventions(Action<RabbitMqMessageRoutingConvention> configure)
        {
            _host = JasperHost.For(opts =>
            {
                opts.UseRabbitMq().UseConventionalRouting(configure).AutoProvision().AutoPurgeOnStartup();
            });
        }

        internal IMessageRouter RoutingFor<T>()
        {
            return theRuntime.RoutingFor(typeof(T));
        }

        internal IJasperRuntime theRuntime
        {
            get
            {
                if (_host == null)
                {
                    _host = JasperHost.For(opts => opts.UseRabbitMq().UseConventionalRouting().AutoProvision().AutoPurgeOnStartup());
                }

                return _host.Services.GetRequiredService<IJasperRuntime>();
            }
        }

        internal void AssertNoRoutes<T>()
        {
            RoutingFor<T>().ShouldBeOfType<EmptyMessageRouter<T>>();
        }

        internal MessageRoute[] PublishingRoutesFor<T>()
        {
            return RoutingFor<T>().ShouldBeOfType<MessageRouter<T>>().Routes;
        }

        internal RabbitMqTransport theTransport
        {
            get
            {
                if (_host == null)
                {
                    _host = JasperHost.For(opts => opts.UseRabbitMq().UseConventionalRouting());
                }

                var options = _host.Services.GetRequiredService<IJasperRuntime>().Options;

                return options.RabbitMqTransport();
            }
        }

    }
}