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

        public TransportNode(ChannelGraph graph, string machineName)
        {
            NodeName = graph.Name;
            ControlChannel = graph.ControlChannel?.Uri ?? graph.FirstOrDefault(x => x.Incoming)?.Uri;
            MachineName = machineName;
            Id = $"{NodeName}@{machineName}";
        }

        public string NodeName { get; set; }
        public string Id { get; set; }

        [Obsolete("Think this will be obsolete w/ the address")]
        public string MachineName { get; set; }

        public Uri ControlChannel { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, MachineName: {MachineName}, NodeName: {NodeName}";
        }
    }
}
