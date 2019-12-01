using System;
using Jasper.Configuration;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpEndpoint : Endpoint
    {
        public TcpEndpoint()
        {
        }

        public TcpEndpoint(int port)
        {
            Port = port;

        }

        public TcpEndpoint(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        public string HostName { get; set; } = "localhost";

        public int Port { get; private set; }

        public override void Parse(Uri uri)
        {
            if (uri.Scheme != "tcp")
            {
                throw new ArgumentOutOfRangeException(nameof(uri));
            }

            HostName = uri.Host;
            Port = uri.Port;

            if (uri.IsDurable())
            {
                IsDurable = true;
            }
        }
    }
}
