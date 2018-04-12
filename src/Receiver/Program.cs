using System;
using System.Threading.Tasks;
using Jasper.CommandLine;
using Jasper.Messaging.Transports.Configuration;

namespace Receiver
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<ReceiverApp>(args);
        }
    }
}
