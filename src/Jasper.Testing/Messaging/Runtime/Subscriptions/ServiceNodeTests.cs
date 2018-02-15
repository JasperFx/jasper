using System;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime.Subscriptions
{
    public class ServiceNodeTests
    {
        [Fact]
        public void use_http_endpoint_as_local_uri_if_exists()
        {
            var node = new ServiceNode
            {
                HttpEndpoints = new Uri[] {"http://machine1:2456".ToUri()},
                TcpEndpoints = new Uri[]{"tcp://machine1:2678".ToUri()},
                MessagesUrl = "messages"
            };

            node.DetermineLocalUri().ShouldBe("http://machine1:2456/messages".ToUri());
        }


        [Fact]
        public void use_tcp_endpoint_as_local_uri_if_no_http_exists()
        {
            var node = new ServiceNode
            {
                HttpEndpoints = new Uri[0] ,
                TcpEndpoints = new Uri[]{"tcp://machine1:2678".ToUri()},
                MessagesUrl = "messages"
            };

            node.DetermineLocalUri().ShouldBe("tcp://machine1:2678".ToUri());
        }


    }
}
