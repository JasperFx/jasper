using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class default_channel_configuration
    {
        [Fact]
        public async Task use_the_loopback_replies_queue_by_default()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
            });

            try
            {
                var channels = runtime.Get<IChannelGraph>();
                channels.DefaultChannel.Uri.ShouldBe("loopback://replies".ToUri());
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        // SAMPLE: SetDefaultChannel
        public class SetDefaultChannel : JasperRegistry
        {
            public SetDefaultChannel()
            {
                Transports.DefaultIs("loopback://default");
            }
        }
        // ENDSAMPLE

        [Fact]
        public async Task override_the_default_channel()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Transports.DefaultIs("loopback://incoming");
            });

            try
            {
                var channels = runtime.Get<IChannelGraph>();
                channels.DefaultChannel
                    .ShouldBeTheSameAs(channels.GetOrBuildChannel("loopback://incoming".ToUri()));
            }
            finally
            {
                await runtime.Shutdown();
            }
        }


        [Fact]
        public async Task will_route_to_the_default_channel_if_there_is_a_handler_but_no_routes()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Transports.DefaultIs("loopback://incoming");
                _.Handlers.IncludeType<DefaultRoutedMessageHandler>();
            });

            try
            {
                var router = runtime.Get<IMessageRouter>();

                var routes = await router.Route(typeof(DefaultRoutedMessage));

                routes.Single().Destination.ShouldBe("loopback://incoming".ToUri());
            }
            finally
            {
                await runtime.Shutdown();
            }

        }

        [Fact]
        public async Task will_not_route_to_the_default_channel_if_there_is_a_handler_and_routes()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Transports.DefaultIs("loopback://incoming");
                _.Handlers.IncludeType<DefaultRoutedMessageHandler>();

                _.Publish.Message<DefaultRoutedMessage>().To("tcp://localhost:2444/outgoing");
            });

            try
            {
                var router = runtime.Get<IMessageRouter>();

                var routes = await router.Route(typeof(DefaultRoutedMessage));

                routes.Single().Destination.ShouldBe("tcp://localhost:2444/outgoing".ToUri());
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task will_not_route_locally_with_no_handler()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Transports.DefaultIs("loopback://incoming");

            });

            try
            {
                var router = runtime.Get<IMessageRouter>();

                var routes = await router.Route(typeof(DefaultRoutedMessage));

                routes.Any().ShouldBeFalse();
            }
            finally
            {
                await runtime.Shutdown();
            }
        }


    }

    public class DefaultRoutedMessage
    {

    }

    public class DefaultRoutedMessageHandler
    {
        public void Handle(DefaultRoutedMessage message)
        {

        }
    }
}
