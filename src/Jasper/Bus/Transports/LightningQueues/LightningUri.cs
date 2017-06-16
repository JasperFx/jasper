using System;
using System.Linq;

namespace Jasper.Bus.Transports.LightningQueues
{
    public class LightningUri
    {
        public static readonly string Protocol = "lq.tcp";

        public LightningUri(string uriString) : this(new Uri(uriString))
        {

        }

        public LightningUri(Uri address)
        {
            // TODO -- have it do more validation here.
            if (address.Scheme != Protocol)
            {
                throw new ArgumentOutOfRangeException(
                    $"{address.Scheme} is the wrong protocol for a LightningQueue Uri.  Only {Protocol} is accepted");
            }

            Address = address.ToMachineUri();
            Port = address.Port;
            Original = address;
            QueueName = Address.Segments.Last();
        }

        public Uri Address { get; }

        public int Port { get; }

        public string QueueName { get; }
        public Uri Original { get; }
    }
}