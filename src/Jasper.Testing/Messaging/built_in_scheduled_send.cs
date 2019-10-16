using Jasper.Conneg;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class built_in_scheduled_send : IntegrationContext
    {
        public built_in_scheduled_send(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void reader_writer_is_registered()
        {
            Host.Container.ShouldHaveRegistration<IMessageSerializer, EnvelopeReaderWriter>();
            Host.Container.ShouldHaveRegistration<IMessageDeserializer, EnvelopeReaderWriter>();
        }

        [Fact]
        public void handler_graph_already_has_the_scheduled_send_handler()
        {
            var handlers = Host.Get<HandlerGraph>();

            handlers.HandlerFor<Envelope>().ShouldBeOfType<ScheduledSendEnvelopeHandler>();
        }
    }
}
