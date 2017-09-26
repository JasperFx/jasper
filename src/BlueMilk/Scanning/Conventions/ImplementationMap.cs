using System.Linq;
using BlueMilk.Util;

namespace BlueMilk.Scanning.Conventions
{
    public class ImplementationMap : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry registry)
        {
            var interfaces = types.FindTypes(TypeClassification.Interfaces);
            var concretes = types.FindTypes(TypeClassification.Concretes).Where(x => TypeExtensions.HasConstructors(x)).ToArray();

            interfaces.Each(@interface =>
            {
                var implementors = concretes.Where(x => x.CanBeCastTo(@interface)).ToArray();
                if (implementors.Count() == 1)
                {
                    registry.AddType(@interface, implementors.Single());
                }
            });
        }

        public override string ToString()
        {
            return "Register any single implementation of any interface against that interface";
        }
    }
}