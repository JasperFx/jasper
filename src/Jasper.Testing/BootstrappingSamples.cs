using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Jasper.Testing.Configuration;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Testing
{
    public class BootstrappingSamples
    {
        public static async Task AppWithHandlerPolicy()
        {
            #region sample_AppWithHandlerPolicy

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Handlers.GlobalPolicy<WrapWithSimple>();
                }).StartAsync();

            #endregion
        }
    }

    #region sample_WrapWithSimple

    public class WrapWithSimple : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, GenerationRules rules, IContainer container)
        {
            foreach (var chain in graph.Chains) chain.Middleware.Add(new SimpleWrapper());
        }
    }

    #endregion
}
