using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http
{
    public class ServiceNode_determination_specs : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private JasperRuntime _runtime;

        private IServiceNode theServiceNode => _runtime.Node;


        private async Task withApp()
        {
            _runtime = await JasperRuntime.ForAsync(theRegistry);
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        [Fact]
        public async Task capture_service_name_and_machine_name()
        {
            theRegistry.ServiceName = "ImportantService";

            await withApp();

            theServiceNode.ServiceName.ShouldBe("ImportantService");
            theServiceNode.MachineName.ShouldBe(Environment.MachineName);
            theServiceNode.Id.ShouldBe($"ImportantService@{Environment.MachineName}");
        }

        [Fact]
        public async Task capture_overridden_machine_name()
        {
            theRegistry.ServiceName = "ImportantService";
            theRegistry.Settings.Alter<MessagingSettings>(_ =>
            {
                _.MachineName = "BigBox";
            });

            await withApp();

            theServiceNode.ServiceName.ShouldBe("ImportantService");
            theServiceNode.MachineName.ShouldBe("BigBox");
            theServiceNode.Id.ShouldBe("ImportantService@BigBox");
        }

        [Fact]
        public async Task register_http_listener()
        {
            theRegistry.Hosting.UseUrls("http://localhost:5003");

            await withApp();

            var serviceNode = theServiceNode;
            serviceNode.HttpEndpoints.ShouldContain($"http://{Environment.MachineName}:5003".ToUri());
            serviceNode.MessagesUrl.ShouldBe(new MessagingSettings().Http.RelativeUrl);
        }

        [Fact]
        public async Task register_tcp_listener_if_any()
        {

            theRegistry.Transports.LightweightListenerAt(2222);
            theRegistry.Transports.DurableListenerAt(2333);

            await withApp();

            theServiceNode.TcpEndpoints.ShouldContain($"tcp://{Environment.MachineName}:2222".ToUri());
            theServiceNode.TcpEndpoints.ShouldContain($"tcp://{Environment.MachineName}:2333/durable".ToUri());
        }
    }
}
