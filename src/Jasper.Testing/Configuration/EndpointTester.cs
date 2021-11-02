using System;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports.Local;
using Jasper.Transports.Sending;
using Jasper.Transports.Tcp;
using Jasper.Util;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class EndpointTester
    {
        [Fact]
        public void set_name_to_uri_by_default()
        {
            var endpoint = new TcpEndpoint(5000);
            endpoint.Name.ShouldBe("tcp://localhost:5000/");
        }

        public class StandInEndpoint : Endpoint
        {
            public override Uri Uri { get; }
            public override Uri ReplyUri()
            {
                throw new NotImplementedException();
            }

            public override void Parse(Uri uri)
            {

            }

            protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
            {
                throw new NotImplementedException();
            }

            protected override ISender CreateSender(IMessagingRoot root)
            {
                throw new NotImplementedException();
            }
        }




    }
}
