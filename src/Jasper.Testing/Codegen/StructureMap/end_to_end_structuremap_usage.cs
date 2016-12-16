using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Codegen.StructureMap
{
    public class end_to_end_structuremap_usage : CompilationContext
    {
        [Fact]
        public void use_injected_field()
        {
            services.ForSingletonOf<ITouchService>()
                .Use<TouchService>();

            theChain.Call<ITouchService>(x => x.Touch(null));

            theGeneratedCode.ShouldContain($"private {typeof(ITouchService).FullName}");


        }
    }

    public interface ITouchService
    {
        void Touch(MainInput input);
        Task TouchAsync(MainInput input);
    }

    public class TouchService : ITouchService
    {
        public void Touch(MainInput input)
        {
            input.Touch();
        }

        public Task TouchAsync(MainInput input)
        {
            return input.TouchAsync();
        }
    }
}