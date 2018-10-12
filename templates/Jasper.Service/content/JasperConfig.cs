using System;
using Jasper;
using Jasper.CommandLine;

namespace JasperService
{
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            Configuration
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        }
    }

}