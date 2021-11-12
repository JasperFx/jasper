using InteropMessages;
using Jasper;
using Jasper.Configuration;
using Jasper.RabbitMQ;
using Jasper.Tracking;
using Newtonsoft.Json;

namespace InteroperabilityTests
{
    public class JasperApp : JasperOptions
    {
        public JasperApp()
        {
            // application/vnd.masstransit+json
            Advanced.JsonSerialization.TypeNameHandling = TypeNameHandling.All;

            Endpoints.ConfigureRabbitMq(t =>
            {
                t.AutoProvision = true;
                t.AutoPurgeOnStartup = true;
                t.DeclareQueue("jasper"); // TODO -- make this inferred
                t.DeclareQueue("masstransit"); // TODO -- make this inferred

                t.DeclareExchange("jasper", x => x.ExchangeType = ExchangeType.Fanout);
                t.DeclareBinding(new Binding
                {
                    QueueName = "jasper",
                    ExchangeName = "jasper",
                    BindingKey = "jasper"
                });

                t.DeclareExchange("masstransit", x => x.ExchangeType = ExchangeType.Fanout);
                t.DeclareBinding(new Binding
                {
                    QueueName = "masstransit",
                    ExchangeName = "masstransit",
                    BindingKey = "masstransit"
                });
            });

            Endpoints.PublishAllMessages().ToRabbit("masstransit")
                .Advanced(endpoint =>
                {
                    // TODO -- will need access to the RabbitMqTransport to get the reply endpoint, then
                    // write out the MT version of the Uri
                    endpoint.MapOutgoingProperty(x => x.ReplyUri, (e, p) =>
                    {
                        // TODO -- this will need to be cached somehow
                        p.Headers[MassTransitHeaders.ResponseAddress] = "rabbitmq://localhost/jasper";

                    });
                });

            Endpoints.ListenToRabbitQueue("jasper")
                .DefaultIncomingMessage<ResponseMessage>();

            Extensions.UseMessageTrackingTestingSupport();
        }
    }
}
