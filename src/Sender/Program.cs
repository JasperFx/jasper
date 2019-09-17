using System.Threading.Tasks;
using Jasper;
using Jasper.CommandLine;

namespace Sender
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            return JasperHost.Run<SenderApp>(args);
        }
    }
}
