using System.Threading.Tasks;
using Xunit;

namespace Jasper.Testing.Codegen.IoC
{
    public class end_to_end_structuremap_usage : CompilationContext
    {
        [Fact]
        public async Task use_injected_field()
        {
            services.ForSingletonOf<ITouchService>()
                .Use<TouchService>();

            theChain.Call<ITouchService>(x => x.TouchAsync(null));

            theGeneratedCode
                .ShouldContain($"private readonly {typeof(ITouchService).FullName}");

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();
        }




        [Fact]
        public async Task use_variable_created_from_nested_container()
        {
            services.For<ITouchService>().Use<TouchService>();

            theChain.Call<ITouchService>(x => x.Touch(null));

            theGeneratedCode
                .ShouldContain($"using (var nested = _root.GetNestedContainer())");

            theGeneratedCode.ShouldContain("var touchService = nested.GetInstance<Jasper.Testing.Codegen.IoC.ITouchService>();");

            var input = await afterRunning();

            input.WasTouched.ShouldBeTrue();
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