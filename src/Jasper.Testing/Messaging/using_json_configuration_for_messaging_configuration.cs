using System.Linq;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Shouldly;
using TestMessages;
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
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) => config.AddJsonFile("messaging.json"))
                .UseJasper();

            using (var runtime = builder.Start())
            {
                var settings = runtime.Services.GetService<JasperOptions>();

                // See the messaging.json file
                settings.DisableAllTransports.ShouldBeTrue();
                settings.ScheduledJobs.PollingTime.ShouldBe(10.Seconds());
                settings.Listeners.Select(x => x.Uri).Contains("tcp://localhost:2000".ToUri()).ShouldBeTrue();
                settings.Subscriptions.Contains(Subscription.All("tcp://localhost:2002".ToUri())).ShouldBeTrue();
            }
        }


        [Fact]
        public void try_stuff()
        {
            var options = new JasperOptions
            {
                ThrowOnValidationErrors = false,
                Listeners = new[] {"tcp://localhost:2000".ToUri(), "tcp://localhost:2001".ToUri()}.Select(x => new ListenerSettings{Uri = x}).ToArray(),
                Subscriptions = new[]
                {
                    Subscription.All("tcp://localhost:2002".ToUri()),
                    Subscription.ForType<Message1>("tcp://localhost:2004".ToUri())
                }
            };

            var json = JsonConvert.SerializeObject(options, Formatting.Indented);

            _output.WriteLine(json);
        }
    }
}
