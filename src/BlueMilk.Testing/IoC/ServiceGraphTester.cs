using System.Linq;
using Jasper.Internals;
using Jasper.Internals.Codegen;
using Jasper.Internals.IoC;
using Jasper.Testing.Internals.TargetTypes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Internals.IoC
{
    public class ServiceGraphTester
    {
        public readonly ServiceRegistry theServices = new ServiceRegistry();
        private readonly ServiceGraph theGraph;

        public ServiceGraphTester()
        {
            theGraph = new ServiceGraph(theServices);
        }

        [Fact]
        public void finds_the_single_default()
        {
            theServices.AddTransient<IWidget, AWidget>();

            theGraph.FindDefault(typeof(IWidget))
                .ImplementationType.ShouldBe(typeof(AWidget));
        }

        [Fact]
        public void finds_the_last_as_the_default()
        {
            theServices.AddTransient<IWidget, AWidget>();
            theServices.AddSingleton(this);
            theServices.AddTransient<IGeneratedMethod, GeneratedMethod>();
            theServices.AddTransient<IWidget, MoneyWidget>();

            theGraph.FindDefault(typeof(IWidget))
                .ImplementationType.ShouldBe(typeof(MoneyWidget));

        }

        [Fact]
        public void finds_all()
        {
            theServices.AddTransient<IWidget, AWidget>();
            theServices.AddSingleton(this);
            theServices.AddTransient<IGeneratedMethod, GeneratedMethod>();
            theServices.AddTransient<IWidget, MoneyWidget>();

            theGraph.FindAll(typeof(IWidget))
                .Select(x => x.ImplementationType)
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(MoneyWidget));

        }

        [Fact]
        public void select_greediest_constructor_that_can_be_filled()
        {
            theServices.AddTransient<IWidget, AWidget>();
            theServices.AddSingleton(this);
            theServices.AddTransient<IGeneratedMethod, GeneratedMethod>();
            theServices.AddTransient<IWidget, MoneyWidget>();

            var ctor = theGraph.ChooseConstructor(typeof(DeepConstructorGuy));
            ctor.GetParameters().Select(x => x.ParameterType)
                .ShouldHaveTheSameElementsAs(typeof(IWidget), typeof(IGeneratedMethod));
        }

        [Fact]
        public void will_choose_a_no_arg_ctor_if_that_is_all_there_is()
        {
            var ctor = theGraph.ChooseConstructor(typeof(NoArgGuy));
            ctor.ShouldNotBeNull();
        }

        [Fact]
        public void can_backfill_a_concrete_with_no_args()
        {
            var @default = theGraph.FindDefault(typeof(NoArgGuy));
            @default.ShouldNotBeNull();
            @default.ImplementationType.ShouldBe(typeof(NoArgGuy));
            @default.ServiceType.ShouldBe(typeof(NoArgGuy));
            @default.Lifetime.ShouldBe(ServiceLifetime.Transient);

            theGraph.FindDefault(typeof(NoArgGuy))
                .ShouldBeSameAs(@default);
        }

        [Fact]
        public void can_backfill_a_concrete_that_could_be_handled_by_the_container()
        {
            var @default = theGraph.FindDefault(typeof(DeepConstructorGuy));
            @default.ShouldNotBeNull();
            @default.ImplementationType.ShouldBe(typeof(DeepConstructorGuy));
            @default.ServiceType.ShouldBe(typeof(DeepConstructorGuy));
            @default.Lifetime.ShouldBe(ServiceLifetime.Transient);

            theGraph.FindDefault(typeof(DeepConstructorGuy))
                .ShouldBeSameAs(@default);
        }
    }

    public class NoArgGuy
    {

    }

    public class DeepConstructorGuy
    {
        public DeepConstructorGuy()
        {

        }

        public DeepConstructorGuy(IWidget widget, IGeneratedMethod method)
        {

        }

        public DeepConstructorGuy(IWidget widget, bool nothing)
        {

        }

        public DeepConstructorGuy(IWidget widget, IGeneratedMethod method, IVariableSource source)
        {

        }
    }
}
