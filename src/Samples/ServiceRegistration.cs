using Jasper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Samples
{
    #region sample_JasperAppWithServices
    public class JasperAppWithServices : JasperOptions
    {
        public JasperAppWithServices()
        {
            // Add service registrations with the ASP.Net Core
            // DI abstractions
            Services.AddLogging();

            // or mix and match with StructureMap style
            // registrations
            Services.For(typeof(ILogger)).Use(typeof(Logger<>));
        }
    }
    #endregion


    public interface IThirdPartyService
    {
    }

    public class StubThirdPartyService : IThirdPartyService
    {
    }


}
