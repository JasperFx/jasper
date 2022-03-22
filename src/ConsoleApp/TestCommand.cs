using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using TestingSupport.Compliance;

namespace MyApp
{
    public class TestCommand : OaktonAsyncCommand<NetCoreInput>
    {
        public override async Task<bool> Execute(NetCoreInput input)
        {
            using var host = input.BuildHost();
            await host.Services.GetRequiredService<ICommandBus>().Invoke(new PongMessage());

            return true;
        }
    }
}
