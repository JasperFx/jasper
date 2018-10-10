using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Transports.Configuration;
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
        private readonly ITestOutputHelper _output;

        public using_json_configuration_for_messaging_configuration(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task read_settings_from_json()
        {
            var runtime = await JasperRuntime.ForAsync(x => { x.Configuration.AddJsonFile("messaging.json"); });

            try
            {
                var settings = runtime.Get<MessagingSettings>();

                // See the messaging.json file
                settings.DisableAllTransports.ShouldBeTrue();
                settings.ScheduledJobs.PollingTime.ShouldBe(10.Seconds());
                settings.Listeners.Contains("tcp://localhost:2000".ToUri()).ShouldBeTrue();
                settings.Subscriptions.Contains(Subscription.All("tcp://localhost:2002".ToUri())).ShouldBeTrue();
            }
            finally
            {
                await runtime.Shutdown();
            }
        }


        [Fact]
        public void try_stuff()
        {
            var settings = new MessagingSettings
            {
                ThrowOnValidationErrors = false,
                Listeners = new Uri[] {"tcp://localhost:2000".ToUri(), "tcp://localhost:2001".ToUri()},
                Subscriptions = new Subscription[]
                {
                    Subscription.All("tcp://localhost:2002".ToUri()),
                    Subscription.ForType<Message1>("tcp://localhost:2004".ToUri()),
                }
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            _output.WriteLine(json);


        }
    }
}
