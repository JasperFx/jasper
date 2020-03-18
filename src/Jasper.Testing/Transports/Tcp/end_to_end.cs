using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime.Scheduled;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestingSupport.Compliance;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    [Collection("integration")]
    public class end_to_end : SendingCompliance
    {
        public end_to_end() : base($"tcp://localhost:{++port}/incoming".ToUri())
        {
            SenderIs(x =>
            {


            });

            ReceiverIs(x =>
            {
                x.Endpoints.ListenForMessagesFrom(theAddress);
            });
        }

        private static int port = 2114;

    }
}
