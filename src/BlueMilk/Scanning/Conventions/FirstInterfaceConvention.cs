using System.Linq;
using Baseline;
using BlueMilk.Util;

namespace BlueMilk.Scanning.Conventions
{
    public class FirstInterfaceConvention : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry registry)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes).Where(x => x.HasConstructors()))
            {
                var interfaceType = type.AllInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    registry.AddType(interfaceType, type);
                }
            }

        }

        public override string ToString()
        {
            return "Register all concrete types against the first interface (if any) that they implement";
        }
    }
}
