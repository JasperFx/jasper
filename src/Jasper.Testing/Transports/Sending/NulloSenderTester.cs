using System.Threading.Tasks;
using Jasper.Testing.Messaging;
using Jasper.Transports.Sending;
using Jasper.Util;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Transports.Sending
{
    public class NulloSenderTester
    {
        [Fact]
        public async Task enqueue_automatically_marks_envelope_as_successful()
        {
            var sender = new NulloSender("tcp://localhost:3333".ToUri());

            var callback = Substitute.For<ISenderCallback>();
            sender.Start(callback);

            var env = ObjectMother.Envelope();

            await sender.Send(env);

            callback.Received().Successful(env);
        }
    }
}
