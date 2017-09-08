using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Settings;

namespace Jasper.Consul.Internal
{
    public class ConsulNodeDiscovery : ConsulService, INodeDiscovery
    {
        public const string TRANSPORTNODE_PREFIX = GLOBAL_PREFIX + "node/";

        public ConsulNodeDiscovery(ConsulSettings settings, IChannelGraph channels, BusSettings envSettings) : base(settings, channels, envSettings)
        {
        }


        public Task Register(TransportNode local)
        {
            LocalNode = local;

            var consulKey = $"{TRANSPORTNODE_PREFIX}{LocalNode.ServiceName}/{MachineName}";

            return client.KV.Put(new KVPair(TRANSPORTNODE_PREFIX + "/")
            {
                Value = serialize(LocalNode)
            });
        }

        public async Task<TransportNode[]> FindPeers()
        {
            var nodes = await client.KV.List(TRANSPORTNODE_PREFIX);
            return nodes.Response?.Select(kv => deserialize<TransportNode>(kv.Value)).ToArray() ?? new TransportNode[0];
        }

        public TransportNode LocalNode { get; set; }
    }
}
