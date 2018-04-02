using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime.Subscriptions
{
    public class reading_subscription_data_from_configuration
    {
        [Fact]
        public async Task can_read_from_configuration_by_default()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                // There is subscription data in this file
                _.Configuration.AddJsonFile("subscriptions.json");
            });

            try
            {
                var repository = runtime.Get<ISubscriptionsRepository>();
                var subscriptions = await repository.GetSubscriptions();

                subscriptions.Select(x => x.MessageType)
                    .ShouldHaveTheSameElementsAs(
                        typeof(Message1).ToMessageAlias(),
                        typeof(Message2).ToMessageAlias(),
                        typeof(Message3).ToMessageAlias()
                        );
            }
            finally
            {
                await runtime.Shutdown();
            }
        }
    }
}
