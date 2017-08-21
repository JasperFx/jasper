using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class ServiceCapabilities
    {
        public string ServiceName { get; set; }
        public PublishedMessage[] Published { get; set; }
        public NewSubscription[] Subscriptions { get; set; }

        public string[] Errors { get; set; } = new string[0];

        public bool Publishes<T>()
        {
            return Published.Any(x => x.DotNetType == typeof(T));
        }

        public async Task ApplyLookups(UriAliasLookup lookups)
        {
            var all = Subscriptions.Select(x => x.Destination).Distinct().ToArray();
            await lookups.ReadAliases(all);

            foreach (var subscription in Subscriptions)
            {
                subscription.Destination = lookups.Resolve(subscription.Destination);
            }
        }
    }
}
