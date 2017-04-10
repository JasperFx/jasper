using System;
using Jasper;
using JasperServer;

namespace JasperServerHarness
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (JasperRuntime.For<MyApp>())
            {
                Console.Read();
            }
        }
    }

    public class MyApp : HybridJasperRegistry
    {
        public MyApp()
        {
            Host.Port = 5000;
        }
    }
}
