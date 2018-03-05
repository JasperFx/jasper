using System.Linq;
using Lamar.Scanning;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
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

        public void ScanTypes(TypeSet types, IServiceCollection services)
        {
            var forwardingTypes = types.FindTypes(TypeClassification.Closed)
                .Where(t => TypeExtensions.Closes(t, typeof(IForwardsTo<>)));

            foreach (var type in forwardingTypes)
            {
                _forwarders.Add(type);
            }
        }
    }
}
