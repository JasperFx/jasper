using Jasper;
using Jasper.Configuration;

namespace Module1
{
    public class Module1Extension : IJasperExtension
    {
        public static JasperOptions Options { get; set; }

        public void Configure(JasperOptions options)
        {
            Options = options;

            options.Services.For<IModuleService>().Use<ServiceFromModule>();
        }
    }

    public interface IModuleService
    {
    }


    public class ServiceFromModule : IModuleService
    {
    }
}
