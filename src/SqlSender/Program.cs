using Jasper.CommandLine;

namespace SqlSender
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperAgent.Run<SenderApp>(args);
        }
    }
}
