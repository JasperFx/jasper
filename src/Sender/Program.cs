using Jasper;
using Jasper.CommandLine;

namespace Sender
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperHost.Run<SenderApp>(args);
        }
    }
}
