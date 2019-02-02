using System;
using System.Collections.Generic;
using System.Linq;
using Jasper;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace MessagingTests.Configuration
{
    public class registering_subscriptions_against_lazy_usage_of_config_key : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private IDictionary<string, string> theConfigKeys = new Dictionary<string, string>();
        private IJasperHost _host;


        private JasperOptions theOptions
        {
            get
            {
                if (_host == null)
                {
                    _host = JasperHost.CreateDefaultBuilder()
                        .ConfigureAppConfiguration((c, builder) => builder.AddInMemoryCollection(theConfigKeys))
                        .UseJasper(theRegistry)
                        .StartJasper();

                }

                return _host.Get<JasperOptions>();
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        [Fact]
        public void all_messages_to_config_key()
        {
            var uriString = "tcp://server:2000";

            theConfigKeys.Add("destination", uriString);

            theRegistry.Publish.AllMessagesToUriValueInConfig("destination");

            var subscription = theOptions.Subscriptions.Single();
            subscription.Scope.ShouldBe(RoutingScope.All);
            subscription.Uri.ShouldBe(uriString.ToUri());
        }

        [Fact]
        public void messages_by_namespace_and_config_key()
        {
            var uriString = "tcp://server:2000";

            theConfigKeys.Add("outgoing", uriString);

            theRegistry.Publish.MessagesFromNamespace("Foo")
                .ToUriValueInConfig("outgoing");

            var subscription = theOptions.Subscriptions.Single();
            subscription.Scope.ShouldBe(RoutingScope.Namespace);
            subscription.Match.ShouldBe("Foo");
            subscription.Uri.ShouldBe(uriString.ToUri());
        }

        [Fact]
        public void listen_at_config_value()
        {
            var uriString = "tcp://server:2000";

            theConfigKeys.Add("listener", uriString);

            theRegistry.Transports.ListenForMessagesFromUriValueInConfig("listener");

            theOptions.Listeners
                .ShouldContain(uriString.ToUri());

        }
    }
}
