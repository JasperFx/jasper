using Jasper;
using Jasper.Configuration;
using Jasper.MvcExtender;

[assembly:JasperModule(typeof(MvcExtenderExtension))]

namespace Jasper.MvcExtender
{
    public class MvcExtenderExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {

        }
    }
}
