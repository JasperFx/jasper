using Jasper;
using Jasper.RabbitMQ;
using Jasper.Tracking;

namespace InteroperabilityTests
{
    public class JasperApp : JasperOptions
    {
        public JasperApp()
        {
            Endpoints.ConfigureRabbitMq(t =>
            {
                t.AutoProvision = true;
                t.AutoPurgeOnStartup = true;
                t.DeclareQueue("jasper"); // TODO -- make this inferred
                t.DeclareQueue("masstransit"); // TODO -- make this inferred
            });

            Endpoints.PublishAllMessages().ToRabbit("masstransit");
            Endpoints.ListenToRabbitQueue("jasper");

            Extensions.UseMessageTrackingTestingSupport();
        }
    }
}
