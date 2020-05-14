using System.Threading;
using System.Threading.Tasks;
using Jasper.Testing.Messaging;
using Jasper.Transports.Sending;
using Jasper.Util;
using Xunit;

namespace Jasper.Testing.Transports.Sending
{
    public class NulloSenderTester
    {
        [Fact]
        public async Task enqueue_automatically_marks_envelope_as_successful()
        {
            var sender = new NulloSender("tcp://localhost:3333".ToUri());

            var env = ObjectMother.Envelope();

            await sender.Send(env);
        }
    }
}
