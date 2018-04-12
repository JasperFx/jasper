using Jasper.CommandLine;
using Jasper.Messaging.Transports.Configuration;

namespace Sender
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<SenderApp>(args);
        }
    }
}
