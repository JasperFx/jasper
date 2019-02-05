using System;
using Jasper;
using Jasper.CommandLine;
using Microsoft.Extensions.Configuration;

namespace JasperHttp
{
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            // Add any necessary jasper options

            // You can also register Lamar service registrations here too
            // Services.For<ISomeService>().Use<SomeService>();
        }
    }

}