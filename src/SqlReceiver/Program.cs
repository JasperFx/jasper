using Jasper;
using Jasper.CommandLine;

namespace SqlReceiver
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperHost.Run<ReceiverApp>(args);
        }
    }
}
