using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Testing.Configuration;
using LamarCodeGeneration.Util;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Local
{
    public class configuring_local_queues_in_JasperOptions
    {
        [Fact]
        public void configure_the_default_queue()
        {
            var options = new JasperOptions();

            var localTransport = options.Endpoints.GetTransport<LocalTransport>();
            options.LocalQueues.Default()
                .ShouldBeSameAs(localTransport.QueueFor(TransportConstants.Default));
        }

        [Fact]
        public void configure_the_replies_queue()
        {
            var options = new JasperOptions();

            var localTransport = options.Endpoints.GetTransport<LocalTransport>();
            options.LocalQueues.ByName(TransportConstants.Replies)
                .ShouldBeSameAs(localTransport.QueueFor(TransportConstants.Replies));
        }

        [Fact]
        public void create_new_queue_on_demand()
        {
            var options = new JasperOptions();
            var one = options.LocalQueues.ByName("One");

            var localTransport = options.Endpoints.GetTransport<LocalTransport>();
            one.ShouldBeSameAs(localTransport.QueueFor("one"));
        }

        [Fact]
        public void public_to_with_durable()
        {
            var options = new JasperOptions();
            options.Publish.AllMessagesTo("local://one/durable");

            options.LocalQueues.ByName("one")
                .As<LocalQueueSettings>()
                .IsDurable
                .ShouldBeTrue();
        }

        [Fact]
        public void public_to_with_not_durable()
        {
            var options = new JasperOptions();
            options.Publish.AllMessagesTo("local://one");

            options.LocalQueues.ByName("one")
                .As<LocalQueueSettings>()
                .IsDurable
                .ShouldBeFalse();
        }

    }
}
