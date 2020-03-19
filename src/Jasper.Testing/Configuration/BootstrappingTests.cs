using System;
using System.Linq;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module1;
using Shouldly;
using TestingSupport;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class BootstrappingTests : IntegrationContext
    {
        public BootstrappingTests(DefaultApp @default) : base(@default)
        {
        }


        [Fact]
        public void can_apply_a_wrapper_to_all_chains()
        {
            with(_ => _.Handlers.GlobalPolicy<WrapWithSimple>());

            chainFor<MovieAdded>().Middleware.OfType<SimpleWrapper>().Any().ShouldBeTrue();
        }

        [Fact]
        public void can_build_i_message_context()
        {
            Host.Get<IMessageContext>().ShouldNotBeNull();

            Host.Get<ThingThatUsesContext>()
                .Context.ShouldNotBeNull();
        }

        public class ThingThatUsesContext
        {
            public IMessageContext Context { get; }

            public ThingThatUsesContext(IMessageContext context)
            {
                Context = context;
            }
        }

        [Fact]
        public void can_customize_source_code_generation()
        {
            with(_ =>
            {
                _.Advanced.CodeGeneration.Sources.Add(new SpecialServiceSource());
                _.Handlers.IncludeType<SpecialServiceUsingThing>();
            });


            chainFor<Message1>()
                .ShouldHaveHandler<SpecialServiceUsingThing>(x => x.Handle(null, null));
        }

        public class AppWithOverrides : JasperOptions
        {
            public AppWithOverrides()
            {
                Handlers.DisableConventionalDiscovery();

                Services.For<IModuleService>().Use<AppsModuleService>();
            }
        }

        public class AppsModuleService : IModuleService
        {
        }

        [Fact]
        public void application_service_registrations_win()
        {
            using (var runtime = JasperHost.For<AppWithOverrides>())
            {
                runtime.Get<IContainer>().DefaultRegistrationIs<IModuleService, AppsModuleService>();

            }
        }

        public class SomeMessage
        {
        }

        public class SomeHandler
        {
            public void Handle(SomeMessage message)
            {
            }
        }

        [Fact]
        public void handler_classes_are_scoped()
        {
            // forcing the container to resolve the family
            var endpoint = Host.Get<SomeHandler>();

            Host.Get<IContainer>().Model.For<SomeHandler>().Default
                .Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }

        [Fact]
        public void with_aspnet_core()
        {

            var options = Host.Get<IOptions<LoggerFilterOptions>>();
            var logging = options.Value;


            var logger = Host.Get<ILogger<Thing>>();


            logger.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(typeof(ICommandBus))]
        [InlineData(typeof(IMessageContext))]
        [InlineData(typeof(IMessagePublisher))]
        public void can_build_services(Type serviceType)
        {
            Host.Get(serviceType)
                .ShouldNotBeNull();
        }

        [Fact]
        public void reader_writer_is_registered()
        {
            Host.Get<IContainer>().ShouldHaveRegistration<IMessageSerializer, EnvelopeReaderWriter>();
            Host.Get<IContainer>().ShouldHaveRegistration<IMessageDeserializer, EnvelopeReaderWriter>();
        }

        [Fact]
        public void handler_graph_already_has_the_scheduled_send_handler()
        {
            var handlers = Host.Get<HandlerGraph>();

            handlers.HandlerFor<Envelope>().ShouldBeOfType<ScheduledSendEnvelopeHandler>();
        }

    }

    public class Thing
    {
        public Thing(ILogger<Thing> logger)
        {
            Logger = logger;
        }

        public ILogger<Thing> Logger { get; }
    }

    // SAMPLE: AppWithHandlerPolicy
    public class AppWithHandlerPolicy : JasperOptions
    {
        public AppWithHandlerPolicy()
        {
            Handlers.GlobalPolicy<WrapWithSimple>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: WrapWithSimple
    public class WrapWithSimple : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, GenerationRules rules, IContainer container)
        {
            foreach (var chain in graph.Chains)
            {
                chain.Middleware.Add(new SimpleWrapper());
            }
        }
    }
    // ENDSAMPLE

    public class SimpleWrapper : Frame
    {
        public SimpleWrapper() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// Just a comment that SimpleWrapper was there");

            Next?.GenerateCode(method, writer);
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
