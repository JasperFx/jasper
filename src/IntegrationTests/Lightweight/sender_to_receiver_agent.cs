using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationTests.Lightweight.Protocol;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Net;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Lightweight;
using Shouldly;
using Xunit;

namespace IntegrationTests.Lightweight
{
    public class sender_to_receiver_agent : IDisposable
    {
        private readonly RecordingReceiverCallback theReceiver = new RecordingReceiverCallback();
        private ListeningAgent theListener;
        private Uri destination = $"lq.tcp://localhost:2113/incoming".ToUri();
        private SendingAgent theSender;

        public sender_to_receiver_agent()
        {
            theListener = new ListeningAgent(theReceiver, 2113);
            theSender = new SendingAgent();

            theListener.Start();
            theSender.Start(new StubSenderCallback());
        }

        private OutgoingMessage outgoingMessage()
        {
            return new OutgoingMessage
            {
                Id = MessageId.GenerateRandom(),
                Destination = destination,
                Data = new byte[]{1,2,3,4,5,6,7},
                Queue = "outgoing",
                SentAt = DateTime.Today.ToUniversalTime()
            };
        }

        public void Dispose()
        {
            theListener.Dispose();
            theSender.Dispose();
        }

        [Fact]
        public async Task send_and_receive_a_single_message()
        {
            theReceiver.ExpectCount = 1;

            var outgoing = outgoingMessage();

            theSender.Enqueue(outgoing);

            await theReceiver.Completed;

            theReceiver.ReceivedMessages.Single().Id.ShouldBe(outgoing.Id);
        }


        [Fact]
        public async Task send_several_messages()
        {
            theReceiver.ExpectCount = 100;

            for (int i = 0; i < 100; i++)
            {
                theSender.Enqueue(outgoingMessage());
            }

            await theReceiver.Completed;

            theReceiver.ReceivedMessages.Count.ShouldBe(100);
        }
    }

    public class RecordingReceiverCallback : IReceiverCallback
    {
        public readonly List<Message> ReceivedMessages = new List<Message>();

        public int ExpectCount { get; set; }

        public ReceivedStatus Received(Message[] messages)
        {
            ReceivedMessages.AddRange(messages);

            if (ReceivedMessages.Count >= ExpectCount)
            {
                _expected.SetResult(true);
            }

            return ReceivedStatus.Successful;
        }

        private readonly TaskCompletionSource<bool> _expected
            = new TaskCompletionSource<bool>();

        public Task Completed => _expected.Task;

        public void Acknowledged(Message[] messages)
        {

        }

        public void NotAcknowledged(Message[] messages)
        {

        }

        public void Failed(Exception exception, Message[] messages)
        {

        }
    }

    public class NulloSenderCallback : ISenderCallback
    {
        public void Successful(OutgoingMessageBatch outgoing)
        {

        }

        public void TimedOut(OutgoingMessageBatch outgoing)
        {

        }

        public void SerializationFailure(OutgoingMessageBatch outgoing)
        {

        }

        public void QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {

        }

        public void ProcessingFailure(OutgoingMessageBatch outgoing)
        {

        }

        public void ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            throw exception;
        }
    }
}
