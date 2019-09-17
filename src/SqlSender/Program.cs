using System.Threading.Tasks;
using Jasper;

namespace SqlSender
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            return JasperHost.Run<SenderApp>(args);
        }
    }
}
