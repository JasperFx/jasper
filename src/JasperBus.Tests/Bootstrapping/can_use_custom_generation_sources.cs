using System;
using Jasper.Codegen;
using JasperBus.Tests.Runtime;
using Xunit;

namespace JasperBus.Tests.Bootstrapping

{
    public class can_use_custom_generation_sources : IntegrationContext
    {
        [Fact]
        public void can_customize_source_code_generation()
        {
            with(_ =>
            {
                _.Generation.Sources.Add(new SpecialServiceSource());
            });

            chainFor<Message1>().ShouldHaveHandler<SpecialServiceUsingHandler>(x => x.Handle(null, null));
        }
    }

    public class SpecialServiceUsingHandler
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