using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Samples
{
    // SAMPLE: MyMissingHandler
    public class MyMissingHandler : IMissingHandler
    {
        public Task Handle(Envelope envelope, IEnvelopeContext context)
        {
            return context
                .SendFailureAcknowledgement(envelope, "I don't know how to process this message");
        }
    }
    // ENDSAMPLE

    // SAMPLE: ConfigureMissingHandler
    public class ConfigureMissingHandler : JasperRegistry
    {
        public ConfigureMissingHandler()
        {
            // Just add your type to the IoC container
            Services.AddSingleton<IMissingHandler, MyMissingHandler>();
        }
    }
    // ENDSAMPLE




}
