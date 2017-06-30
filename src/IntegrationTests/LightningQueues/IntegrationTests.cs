using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Queues
{
    [Collection("SharedTestDirectory")]
    public class IntegrationTests : IDisposable
    {
        private Queue _receiver;
        private Queue _sender;
        private readonly RecordingLogger _senderLogger;

        public IntegrationTests(SharedTestDirectory testDirectory)
        {
            _senderLogger = new RecordingLogger();
            _receiver = ObjectMother.NewQueue(testDirectory.CreateNewDirectoryForTest(), "receiver");
            _sender = ObjectMother.NewQueue(testDirectory.CreateNewDirectoryForTest(), "sender", _senderLogger);
        }

        [Fact(Skip = "Fails only on linux... Will have to investigate later")]
        public async Task can_send_and_receive_after_queue_not_found()
        {
            var message = ObjectMother.NewMessage<Envelope>("receiver2");
            message.Destination = new Uri($"lq.tcp://localhost:{_receiver.Endpoint.Port}");
            _sender.Send(message);
            message.Queue = "receiver";
            _sender.Send(message);
            var received = await _receiver.Receive("receiver").FirstAsyncWithTimeout();
            ShouldBeNullExtensions.ShouldNotBeNull(received);
            received.Message.Queue.ShouldBe(message.Queue);
            received.Message.Data.ShouldBe(message.Data);
            ShouldBeBooleanExtensions.ShouldBeTrue(_senderLogger.DebugMessages.Any(x => x.Contains("Got an error sending")));
        }

        public void Dispose()
        {
            _sender.Dispose();
            _receiver.Dispose();
        }
    }
}
