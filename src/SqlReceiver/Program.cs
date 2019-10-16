using System.Threading.Tasks;
using Jasper;

namespace SqlReceiver
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            return JasperHost.Run<ReceiverApp>(args);
        }
    }
}
