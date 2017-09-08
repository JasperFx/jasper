using System;
using System.Linq;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class TransportNode
    {
        //For json deserialization
        public TransportNode()
        {
        }

        public TransportNode(IChannelGraph graph, string machineName)
        {
            ServiceName = graph.Name;

            MachineName = machineName;
            Id = $"{ServiceName}@{machineName}";
        }

        public string ServiceName { get; set; }
        public string Id { get; set; }

        [Obsolete("Think this will be obsolete w/ the address")]
        public string MachineName { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, MachineName: {MachineName}, NodeName: {ServiceName}";
        }
    }
}
