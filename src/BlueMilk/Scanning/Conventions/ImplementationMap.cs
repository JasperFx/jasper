using System.Linq;
using Baseline;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Scanning.Conventions
{
    public class ImplementationMap : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, IServiceCollection services)
        {
            var interfaces = types.FindTypes(TypeClassification.Interfaces);
            var concretes = types.FindTypes(TypeClassification.Concretes).Where(x => x.GetConstructors().Any()).ToArray();

            interfaces.Each(@interface =>
            {
                var implementors = concretes.Where(x => x.CanBeCastTo(@interface)).ToArray();
                if (implementors.Count() == 1)
                {
                    services.AddType(@interface, implementors.Single());
                }
            });
        }

        public override string ToString()
        {
            return "Register any single implementation of any interface against that interface";
        }
    }
}
