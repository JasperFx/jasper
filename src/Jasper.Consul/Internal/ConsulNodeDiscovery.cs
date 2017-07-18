using System.Collections.Generic;
using System.Linq;
using Consul;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Consul.Internal
{
    public class ConsulNodeDiscovery : ConsulService, INodeDiscovery
    {
        public const string TRANSPORTNODE_PREFIX = GLOBAL_PREFIX + "node/";

        public ConsulNodeDiscovery(ConsulSettings settings, ChannelGraph channels, EnvironmentSettings envSettings) : base(settings, channels, envSettings)
        {
        }


        public void Register(ChannelGraph graph)
        {
            LocalNode = new TransportNode(graph, MachineName);

            var consulKey = $"{TRANSPORTNODE_PREFIX}{LocalNode.NodeName}/{MachineName}";

            client.KV.Put(new KVPair(TRANSPORTNODE_PREFIX + "/")
            {
                Value = serialize(LocalNode)
            }).Wait();
        }

        public IEnumerable<TransportNode> FindPeers()
        {
            var nodes = client.KV.List(TRANSPORTNODE_PREFIX).Result;
            return nodes.Response?.Select(kv => deserialize<TransportNode>(kv.Value)) ?? new TransportNode[0];
        }

        public TransportNode LocalNode { get; set; }
    }
}
