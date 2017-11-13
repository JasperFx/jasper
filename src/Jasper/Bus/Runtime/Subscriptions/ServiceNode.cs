using System;
using System.Linq;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface IServiceNode
    {
        string ServiceName { get; }
        string NodeId { get; }
        string MachineName { get; }
        Uri[] HttpEndpoints { get; }
        string MessagesUrl { get;  }
        Uri[] TcpEndpoints { get; }
    }

    public class ServiceNode : IServiceNode
    {
        //For json deserialization
        public ServiceNode()
        {
        }

        public ServiceNode(BusSettings settings)
        {
            ServiceName = settings.ServiceName;

            MachineName = settings.MachineName;
            NodeId = $"{ServiceName}@{MachineName}";

            MessagesUrl = settings.Http.RelativeUrl;


            TcpEndpoints = settings.Listeners.Where(x => x.Scheme == "tcp")
                .Select(x => x.ToCanonicalUri().ToMachineUri()).Distinct()
                .ToArray();
        }

        public string ServiceName { get; set; }
        public string NodeId { get; set; }

        public string MachineName { get; set; }

        public Uri[] HttpEndpoints { get; set; }

        public string MessagesUrl { get; set; }

        public Uri[] TcpEndpoints { get; set; }

        public override string ToString()
        {
            return $"Id: {NodeId}, MachineName: {MachineName}, NodeName: {ServiceName}";
        }
    }
}
