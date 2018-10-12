using System;
using Jasper;
using Jasper.CommandLine;
using Microsoft.Extensions.Configuration;

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