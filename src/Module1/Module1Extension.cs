using Jasper;
using Jasper.Configuration;

namespace Module1
{
    public class Module1Extension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            Registry = registry;
        }

        public static JasperRegistry Registry { get; set; }
    }
}