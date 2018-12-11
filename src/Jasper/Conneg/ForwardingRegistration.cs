using System.Linq;
using Lamar;
using Lamar.Scanning;
using Lamar.Scanning.Conventions;
using TypeExtensions = Baseline.TypeExtensions;

namespace Jasper.Conneg
{
    public class ForwardingRegistration : IRegistrationConvention
    {
        private readonly Forwarders _forwarders;

        public ForwardingRegistration(Forwarders forwarders)
        {
            _forwarders = forwarders;
        }

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            var forwardingTypes = types.FindTypes(TypeClassification.Closed)
                .Where(t => TypeExtensions.Closes(t, typeof(IForwardsTo<>)));

            foreach (var type in forwardingTypes) _forwarders.Add(type);
        }
    }
}
