using System;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class TransportNode
    {
        //For json deserialization
        public TransportNode()
        {
        }

        public TransportNode(BusSettings settings)
        {
            ServiceName = settings.ServiceName;

            MachineName = settings.MachineName;
            Id = $"{ServiceName}@{MachineName}";
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
