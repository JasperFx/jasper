using System;
using System.Linq;
using Baseline;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Scanning.Conventions
{
    public class FirstInterfaceConvention : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, IServiceCollection services)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes).Where(x => x.GetConstructors().Any()))
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault(x => x != typeof(IDisposable));
                if (interfaceType != null && !interfaceType.HasAttribute<BlueMilkIgnoreAttribute>())
                {
                    services.AddType(interfaceType, type);
                }
            }

        }

        public override string ToString()
        {
            return "Register all concrete types against the first interface (if any) that they implement";
        }
    }
}
