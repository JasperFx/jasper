using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Testing.Messaging
{
    public class using_json_configuration_for_messaging_configuration
    {
        public using_json_configuration_for_messaging_configuration(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Fact]
        public void read_settings_from_json()
        {
            var builder = JasperHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) => config.AddJsonFile("messaging.json"))
                .UseJasper();

            using (var runtime = builder.StartJasper())
            {
                var settings = runtime.Get<JasperOptions>();

                // See the messaging.json file
                settings.DisableAllTransports.ShouldBeTrue();
                settings.ScheduledJobs.PollingTime.ShouldBe(10.Seconds());
                settings.Listeners.Contains("tcp://localhost:2000".ToUri()).ShouldBeTrue();
                settings.Subscriptions.Contains(Subscription.All("tcp://localhost:2002".ToUri())).ShouldBeTrue();
            }
        }


        [Fact]
        public void try_stuff()
        {
            var settings = new JasperOptions
            {
                ThrowOnValidationErrors = false,
                Listeners = new[] {"tcp://localhost:2000".ToUri(), "tcp://localhost:2001".ToUri()},
                Subscriptions = new[]
                {
                    Subscription.All("tcp://localhost:2002".ToUri()),
                    Subscription.ForType<Message1>("tcp://localhost:2004".ToUri())
                }
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            _output.WriteLine(json);
        }
    }
}
