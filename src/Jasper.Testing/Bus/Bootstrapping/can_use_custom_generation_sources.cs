using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using Jasper.Testing.Bus.Runtime;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping

{
    public class can_use_custom_generation_sources : IntegrationContext
    {
        [Fact]
        public void can_customize_source_code_generation()
        {
            with(_ =>
            {
                _.Generation.Sources.Add(new SpecialServiceSource());
                _.Handlers.IncludeType<SpecialServiceUsingThing>();
            });



            chainFor<Message1>().ShouldHaveHandler<SpecialServiceUsingThing>(x => x.Handle(null, null));
        }
    }

    public class SpecialServiceUsingThing
    {
        public void Handle(Message1 message, SpecialService service)
        {

        }
    }

    public class SpecialServiceSource : StaticVariable
    {
        public SpecialServiceSource() : base(typeof(SpecialService), $"{typeof(SpecialService).FullName}.{nameof(SpecialService.Instance)}")
        {
        }
    }

    public class SpecialService
    {
        public static readonly SpecialService Instance = new SpecialService();

        private SpecialService()
        {
        }
    }
}
