using Jasper.CommandLine;

namespace SqlReceiver
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return JasperAgent.Run<ReceiverApp>(args);
        }
    }
}
