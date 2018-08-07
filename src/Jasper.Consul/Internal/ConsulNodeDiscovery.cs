using System.Linq;
using System.Threading.Tasks;
using Consul;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Consul.Internal
{
    public class ConsulNodeDiscovery : ConsulService, INodeDiscovery
    {
        public const string TransportNodePrefix = GlobalPrefix + "node/";

        public ConsulNodeDiscovery(ConsulSettings settings, MessagingSettings envSettings) :
            base(settings, envSettings)
        {
        }


        public Task Register(ServiceNode local)
        {
            LocalNode = local;

            var consulKey = toConsulKey();

            return client.KV.Put(new KVPair(consulKey)
            {
                Value = serialize(LocalNode)
            });
        }

        public async Task<ServiceNode[]> FindPeers()
        {
            var nodes = await client.KV.List(TransportNodePrefix + LocalNode.ServiceName);
            return nodes.Response?.Select(kv => deserialize<ServiceNode>(kv.Value)).ToArray() ?? new ServiceNode[0];
        }

        public async Task<ServiceNode[]> FindAllKnown()
        {
            var nodes = await client.KV.List(TransportNodePrefix);
            return nodes.Response?.Select(kv => deserialize<ServiceNode>(kv.Value)).ToArray() ?? new ServiceNode[0];
        }

        public ServiceNode LocalNode { get; set; }

        public Task UnregisterLocalNode()
        {
            return client.KV.Delete(toConsulKey());
        }

        private string toConsulKey()
        {
            return $"{TransportNodePrefix}{LocalNode.ServiceName}/{LocalNode.MachineName}";
        }
    }
}
