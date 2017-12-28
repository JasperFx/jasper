using System.Threading.Tasks;
using Jasper.Internals;
using Jasper.Internals.Scanning.Conventions;
using Jasper.Testing.Internals.TargetTypes;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Internals.Scanning.Conventions
{
    public class type_scanning_end_to_end
    {
        [Fact]
        public async Task use_default_scanning()
        {
            var services = new ServiceRegistry();

            services.Scan(x =>
            {
                x.AssemblyContainingType<IShoes>();
                x.WithDefaultConventions();
            });

            await services.ApplyScannedTypes();

            services.FindDefault<IShoes>().ImplementationType.ShouldBe(typeof(Shoes));
            services.FindDefault<IShorts>().ImplementationType.ShouldBe(typeof(Shorts));
        }

        [Fact]
        public async Task single_implementation()
        {
            var services = new ServiceRegistry();

            services.Scan(x =>
            {
                x.AssemblyContainingType<IShoes>();
                x.SingleImplementationsOfInterface();
            });

            await services.ApplyScannedTypes();

            services.FindDefault<Muppet>().ImplementationType.ShouldBe(typeof(Grover));
        }

        [Fact]
        public async Task find_all_implementations()
        {
            var services = new ServiceRegistry();

            services.Scan(x =>
            {
                x.AssemblyContainingType<IShoes>();
                x.AddAllTypesOf<IWidget>();
            });

            await services.ApplyScannedTypes();

            var widgetTypes = services.RegisteredTypesFor<IWidget>();
            widgetTypes.ShouldContain(typeof(MoneyWidget));
            widgetTypes.ShouldContain(typeof(AWidget));
        }
    }



    public interface Muppet
    {

    }

    public class Grover : Muppet
    {

    }

    public interface IShoes
    {

    }

    public class Shoes : IShoes
    {

    }

    public interface IShorts{}
    public class Shorts : IShorts{}
}
