using System.Threading.Tasks;
using Jasper;
using Jasper.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Samples
{
    public static class MissingHandlerSamples
    {
        public static async Task ConfigureMissingHandler()
        {
            #region sample_ConfigureMissingHandler

            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // Just add your type to the IoC container
                    opts.Services.AddSingleton<IMissingHandler, MyMissingHandler>();
                }).StartAsync();

            #endregion
        }
    }

    #region sample_MyMissingHandler

    public class MyMissingHandler : IMissingHandler
    {
        public Task Handle(Envelope? envelope, IJasperRuntime root)
        {
            return root.Acknowledgements
                .SendFailureAcknowledgement(envelope, "I don't know how to process this message");
        }
    }

    #endregion
}
