using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Lmdb;
using Jasper.Bus.Queues.Net.Protocol;
using Jasper.Bus.Queues.Net.Protocol.V1;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Queues.Storage;
using Jasper.Bus.Runtime;
using Microsoft.Reactive.Testing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Queues.Net.Protocol.V1
{
    [Collection("SharedTestDirectory")]
    public class ReceivingProtocolTests : IDisposable
    {
        readonly RecordingLogger _logger;
        readonly ReceivingProtocol _protocol;
        readonly TestScheduler _scheduler;
        readonly IMessageStore _store;

        public ReceivingProtocolTests(SharedTestDirectory testDirectory)
        {
            _logger = new RecordingLogger();
            _scheduler = new TestScheduler();
            _store = new LmdbMessageStore(testDirectory.CreateNewDirectoryForTest());
            _protocol = new ReceivingProtocol(_store, _logger, _scheduler);
        }

        [Fact]
        public void client_sending_negative_length_wont_produce_next_length_item()
        {
            using (var ms = new MemoryStream())
            {
                var subscribeCalled = false;
                ms.Write(BitConverter.GetBytes(-2), 0, 4);
                ms.Position = 0;
                using (_protocol.LengthChunk(ms).Subscribe(x => subscribeCalled = true))
                {
                    ShouldBeBooleanExtensions.ShouldBeFalse(subscribeCalled);
                }
            }
        }

        [Fact]
        public void handling_valid_length()
        {
            var length = 5;
            var actual = -1;
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(length), 0, 4);
                ms.Position = 0;
                using (_protocol.LengthChunk(ms)
                      .Subscribe(x => actual = x))
                {
                    actual.ShouldBe(length);
                }
            }
        }

        [Fact]
        public void sending_shorter_length_than_payload_length()
        {
            runLengthTest(-2);
        }

        [Fact]
        public void sending_longer_length_than_payload_length()
        {
            runLengthTest(5);
        }

        private void runLengthTest(int differenceFromActualLength)
        {
            var message = new Envelope
            {
                Id = MessageId.GenerateRandom(),
                Data = System.Text.Encoding.UTF8.GetBytes("hello"),
                Queue = "test"
            };
            var bytes = new[] { message }.Serialize();
            var subscribeCalled = false;
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(bytes.Length + differenceFromActualLength), 0, 4);
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                using (_protocol.MessagesChunk(ms, bytes.Length)
                      .Subscribe(x => subscribeCalled = true))
                {
                    ShouldBeBooleanExtensions.ShouldBeFalse(subscribeCalled);
                }
            }
        }

        [Fact]
        public async Task storing_to_a_queue_that_doesnt_exist()
        {
            byte[] errorBytes = null;
            using (var ms = new MemoryStream())
            {
                try
                {
                    await _protocol.StoreMessages(ms, ObjectMother.NewMessage<Envelope>("test"));
                }
                catch (QueueDoesNotExistException)
                {
                    ms.Position = 0;
                    errorBytes = await ms.ReadBytesAsync(Constants.QueueDoesNotExistBuffer.Length);
                }
            }
            Constants.QueueDoesNotExistBuffer.ShouldBe(errorBytes);
        }

        [Fact]
        public void sending_to_a_queue_that_doesnt_exist()
        {
            var message = new Envelope
            {
                Id = MessageId.GenerateRandom(),
                Data = System.Text.Encoding.UTF8.GetBytes("hello"),
                Queue = "test"
            };
            var bytes = new[] { message }.Serialize();
            var subscribeCalled = false;
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                using (_protocol.ReceiveStream(Observable.Return(ms), "me").Catch((Exception ex) => Observable.Empty<Envelope>())
                    .Subscribe(x => subscribeCalled = true))
                {
                    ShouldBeBooleanExtensions.ShouldBeFalse(subscribeCalled);
                }
            }
        }

        [Fact]
        public void sending_data_that_is_cannot_be_deserialized()
        {
            using (var ms = new MemoryStream())
            {
                var subscribeCalled = false;
                ms.Write(BitConverter.GetBytes(16), 0, 4);
                ms.Write(Guid.NewGuid().ToByteArray(), 0, 16);
                ms.Position = 0;
                using (_protocol.ReceiveStream(Observable.Return(ms), "me")
                    .Subscribe(x => subscribeCalled = true))
                {
                    ShouldBeBooleanExtensions.ShouldBeTrue(_logger.ErrorMessages.Any(x => x.StartsWith("Error deserializing messages")));
                    ShouldBeBooleanExtensions.ShouldBeFalse(subscribeCalled);
                }
            }
        }

        [Fact]
        public void dealing_with_slow_clients()
        {
            var recording = _scheduler.Start(() => _protocol.ReceiveStream(Observable.Never<Stream>(), "me"),
                TimeSpan.FromSeconds(1).Ticks, TimeSpan.FromSeconds(2).Ticks, TimeSpan.FromSeconds(10).Ticks);

            recording.Messages.First()
                .Value.Exception.ShouldBeOfType<TimeoutException>();
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }
}
