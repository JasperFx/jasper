using System;
using Jasper;
using Jasper.CommandLine;

namespace JasperService
{
    internal class Program
    {
        static int Main(string[] args)
        {
            // The application is configured through the MyApp class
            return JasperAgent.Run<JasperConfig>(args);
        }
    }


}