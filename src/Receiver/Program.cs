using Jasper;
using Jasper.CommandLine;

namespace Receiver
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperHost.Run<ReceiverApp>(args);
        }
    }
}
