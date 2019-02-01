using System;
using Jasper;

namespace JasperService
{
    internal class Program
    {
        static int Main(string[] args)
        {
            // The application is configured through the MyApp clas
            return JasperHost.Run<JasperConfig>(args);
        }
    }


}