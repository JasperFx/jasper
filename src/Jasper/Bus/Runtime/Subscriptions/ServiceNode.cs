using System;
using System.Linq;
using Baseline;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface IServiceNode
    {
        string ServiceName { get; }
        string Id { get; }
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
            Id = $"{ServiceName}@{MachineName}";

            MessagesUrl = settings.Http.RelativeUrl;


            TcpEndpoints = settings.Listeners.Where(x => x.Scheme == "tcp")
                .Select(x => x.ToCanonicalUri().ToMachineUri()).Distinct()
                .ToArray();
        }

        public string ServiceName { get; set; }
        public string Id { get; set; }

        public string MachineName { get; set; }

        public Uri[] HttpEndpoints { get; set; }

        public string MessagesUrl { get; set; }

        public Uri[] TcpEndpoints { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, MachineName: {MachineName}, NodeName: {ServiceName}";
        }

        public Uri DetermineLocalUri()
        {
            if (HttpEndpoints.Any())
            {
                return HttpEndpoints.First().ToString().AppendUrl(MessagesUrl).ToUri();
            }

            return TcpEndpoints.FirstOrDefault();
        }
    }
}
