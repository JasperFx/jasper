using System;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing
{
    public class ServiceNode_determination_specs : IDisposable
    {
        private readonly JasperHttpRegistry theRegistry = new JasperHttpRegistry();
        private readonly Lazy<JasperRuntime> _runtime;

        private IServiceNode theServiceNode => _runtime.Value.Node;

        public ServiceNode_determination_specs()
        {
            _runtime = new Lazy<JasperRuntime>(() => JasperRuntime.For(theRegistry));
        }

        public void Dispose()
        {
            if (_runtime.IsValueCreated) _runtime.Value.Dispose();
        }

        [Fact]
        public void capture_service_name_and_machine_name()
        {
            theRegistry.ServiceName = "ImportantService";

            theServiceNode.ServiceName.ShouldBe("ImportantService");
            theServiceNode.MachineName.ShouldBe(Environment.MachineName);
            theServiceNode.Id.ShouldBe($"ImportantService@{Environment.MachineName}");
        }

        [Fact]
        public void capture_overridden_machine_name()
        {
            theRegistry.ServiceName = "ImportantService";
            theRegistry.Settings.Alter<BusSettings>(_ =>
            {
                _.MachineName = "BigBox";
            });

            theServiceNode.ServiceName.ShouldBe("ImportantService");
            theServiceNode.MachineName.ShouldBe("BigBox");
            theServiceNode.Id.ShouldBe("ImportantService@BigBox");
        }

        [Fact]
        public void register_http_listener()
        {
            theRegistry.Http.UseUrls("http://localhost:5003");

            var serviceNode = theServiceNode;
            serviceNode.HttpEndpoints.ShouldContain($"http://{Environment.MachineName}:5003".ToUri());
            serviceNode.MessagesUrl.ShouldBe(new HttpTransportSettings().RelativeUrl);
        }

        [Fact]
        public void register_tcp_listener_if_any()
        {
            theRegistry.Transports.LightweightListenerAt(2222);
            theRegistry.Transports.DurableListenerAt(2333);

            theServiceNode.TcpEndpoints.ShouldContain($"tcp://{Environment.MachineName}:2222".ToUri());
            theServiceNode.TcpEndpoints.ShouldContain($"tcp://{Environment.MachineName}:2333/durable".ToUri());
        }
    }
}
