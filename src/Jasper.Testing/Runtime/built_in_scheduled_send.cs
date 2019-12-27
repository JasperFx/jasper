using Jasper.Runtime.Handlers;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Lamar;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class built_in_scheduled_send : IntegrationContext
    {
        public built_in_scheduled_send(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void reader_writer_is_registered()
        {
            Host.Get<IContainer>().ShouldHaveRegistration<IMessageSerializer, EnvelopeReaderWriter>();
            Host.Get<IContainer>().ShouldHaveRegistration<IMessageDeserializer, EnvelopeReaderWriter>();
        }

        [Fact]
        public void handler_graph_already_has_the_scheduled_send_handler()
        {
            var handlers = Host.Get<HandlerGraph>();

            handlers.HandlerFor<Envelope>().ShouldBeOfType<ScheduledSendEnvelopeHandler>();
        }
    }
}
