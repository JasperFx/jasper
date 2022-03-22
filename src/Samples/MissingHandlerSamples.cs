using System.Threading.Tasks;
using Jasper;
using Jasper.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Samples
{
    // SAMPLE: MyMissingHandler
    public class MyMissingHandler : IMissingHandler
    {
        public Task Handle(Envelope? envelope, IMessagingRoot root)
        {
            return root.Acknowledgements
                .SendFailureAcknowledgement(envelope,"I don't know how to process this message");
        }
    }
    // ENDSAMPLE

    // SAMPLE: ConfigureMissingHandler
    public class ConfigureMissingHandlerApp : JasperOptions
    {
        public ConfigureMissingHandlerApp()
        {
            // Just add your type to the IoC container
            Services.AddSingleton<IMissingHandler, MyMissingHandler>();
        }
    }

    // ENDSAMPLE
}
