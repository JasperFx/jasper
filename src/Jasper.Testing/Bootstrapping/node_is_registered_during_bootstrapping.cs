using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class node_is_registered_during_bootstrapping
    {
        [Fact]
        public async Task add_and_remove_node_during_bootstrapping()
        {
            var discovery = new StubNodeDiscovery();

            var runtime = await JasperRuntime.ForAsync(x => x.Services.AddSingleton<INodeDiscovery>(discovery));

            var local = runtime.Node;

            discovery.LocalNode.ShouldBeTheSameAs(local);
            discovery.LocalWasRemoved.ShouldBeFalse();

            await runtime.Shutdown();
            discovery.LocalWasRemoved.ShouldBeTrue();
        }
    }

    public class StubNodeDiscovery : INodeDiscovery
    {
        public Task Register(ServiceNode local)
        {
            LocalNode = local;

            return Task.CompletedTask;
        }


        public Task<ServiceNode[]> FindPeers()
        {
            throw new System.NotImplementedException();
        }

        public Task<ServiceNode[]> FindAllKnown()
        {
            throw new System.NotImplementedException();
        }

        public ServiceNode LocalNode { get; set; }

        public Task UnregisterLocalNode()
        {
            LocalWasRemoved = true;
            return Task.CompletedTask;
        }

        public bool LocalWasRemoved { get; set; }
    }
}
