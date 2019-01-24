using Jasper;
using Jasper.CommandLine;

namespace SqlSender
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperHost.Run<SenderApp>(args);
        }
    }
}
