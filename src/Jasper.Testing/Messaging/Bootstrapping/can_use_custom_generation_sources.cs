﻿using LamarCodeGeneration.Model;
using TestingSupport;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping

{
    public class can_use_custom_generation_sources : IntegrationContext
    {
        public can_use_custom_generation_sources(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void can_customize_source_code_generation()
        {
            with(_ =>
            {
                _.CodeGeneration.Sources.Add(new SpecialServiceSource());
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
        public SpecialServiceSource() : base(typeof(SpecialService),
            $"{typeof(SpecialService).FullName}.{nameof(SpecialService.Instance)}")
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
