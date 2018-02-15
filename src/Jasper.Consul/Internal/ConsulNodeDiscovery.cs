using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Jasper.Messaging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Consul.Internal
{
    public class ConsulNodeDiscovery : ConsulService, INodeDiscovery
    {
        public const string TRANSPORTNODE_PREFIX = GLOBAL_PREFIX + "node/";

        public ConsulNodeDiscovery(ConsulSettings settings, IChannelGraph channels, MessagingSettings envSettings) : base(settings, channels, envSettings)
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
            var nodes = await client.KV.List(TRANSPORTNODE_PREFIX + LocalNode.ServiceName);
            return nodes.Response?.Select(kv => deserialize<ServiceNode>(kv.Value)).ToArray() ?? new ServiceNode[0];
        }

        public async Task<ServiceNode[]> FindAllKnown()
        {
            var nodes = await client.KV.List(TRANSPORTNODE_PREFIX);
            return nodes.Response?.Select(kv => deserialize<ServiceNode>(kv.Value)).ToArray() ?? new ServiceNode[0];
        }

        public ServiceNode LocalNode { get; set; }

        public Task UnregisterLocalNode()
        {
            return client.KV.Delete(toConsulKey());
        }

        private string toConsulKey()
        {
            return $"{TRANSPORTNODE_PREFIX}{LocalNode.ServiceName}/{LocalNode.MachineName}";
        }
    }
}
