using Jasper.CommandLine;

namespace SqlReceiver
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<ReceiverApp>(args);
        }
    }
}
