using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jasper.Consul
{
    public class ServiceRegistration
    {
        public const string Unknown = "UNKNOWN";

        [JsonIgnore]
        public string ServiceName { get; private set; }


        public ServiceRegistration()
        {
        }

        public ServiceRegistration(string serviceName, Uri address)
        {
            ServiceName = serviceName;
            Address = address.ToString();
        }

        public string Address { get; set; }

        public string Node
        {
            get => $"jasper/service/{ServiceName}";
            set
            {
                if (value.StartsWith("jasper/service"))
                {
                    var parts = value?.Split('/');
                    ServiceName = parts?[2];
                }
                else
                {
                    ServiceName = Unknown;
                }


            }
        }

        public void AddReplyAddress(Uri replyUri)
        {
            TaggedAddresses[replyUri.Scheme] = replyUri.ToString();
        }

        public Dictionary<string, string> TaggedAddresses { get; set; } = new Dictionary<string, string>();
    }
}