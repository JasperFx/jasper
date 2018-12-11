using Jasper.CommandLine;

namespace Sender
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperAgent.Run<SenderApp>(args);
        }
    }
}
