using System;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports.Local;
using Jasper.Transports.Sending;
using Jasper.Util;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class EndpointTester
    {
        public class StandInEndpoint : Endpoint
        {
            public override Uri ReplyUri()
            {
                throw new NotImplementedException();
            }

            public override void Parse(Uri uri)
            {
                throw new NotImplementedException();
            }

            public override Uri Uri { get; }

            public override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
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
