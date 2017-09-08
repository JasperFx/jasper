using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Settings;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class reading_and_writing_service_capability_information
    {
        [Fact]
        public void can_write_then_read()
        {
            ServiceCapabilities services;

            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);

                _.ServiceName = "AppWithSubscriptions";


                _.Subscriptions.To<Message1>();
                _.Subscriptions.To<Message2>();

                _.Publishing.Message<Message3>();
                _.Publishing.Message<Message4>();
                _.Publishing.Message<Message5>();

                _.Settings.Alter<BusSettings>(x => x.ThrowOnValidationErrors = false);
            }))
            {
                services = runtime.Capabilities;

                services.Errors.Length.ShouldBeGreaterThan(0);
                services.Subscriptions.Length.ShouldBeGreaterThan(0);
                services.Published.Length.ShouldBeGreaterThan(0);
            }

            services.WriteToFile("services.json");

            var services2 = ServiceCapabilities.ReadFromFile("services.json");

            services2.ShouldNotBeNull();
            services2.ServiceName.ShouldBe(services.ServiceName);

            services2.Subscriptions.Length.ShouldBe(2);
            services2.Published.Length.ShouldBe(services.Published.Length);
            services2.Errors.Length.ShouldBe(services.Errors.Length);
        }
    }
}
