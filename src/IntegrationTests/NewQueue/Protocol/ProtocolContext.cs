using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Net;
using Jasper.Bus.Queues.New;
using Jasper.Bus.Runtime;
using Jasper.Testing;
using Shouldly;

namespace IntegrationTests.NewQueue.Protocol
{
    public abstract class ProtocolContext : IDisposable
    {
        protected StubReceiverCallback theReceiver = new StubReceiverCallback();
        protected StubSenderCallback theSender = new StubSenderCallback();
        private readonly IPAddress theAddress = IPAddress.Loopback;
        private readonly int thePort = 2111;
        private TcpListener _listener;
        private Uri destination = "lq.tcp://localhost:2111/incoming".ToUri();
        private OutgoingMessageBatch theMessageBatch;
        private bool _isDisposed;
        private Task _receivingLoop;

        public ProtocolContext()
        {
            _listener = new TcpListener(new IPEndPoint(theAddress, thePort));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);



            var messages = new OutgoingMessage[]
            {
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage()
            };

            theMessageBatch = new OutgoingMessageBatch(destination, messages, new TcpClient());


            _receivingLoop = Task.Run(async () =>
            {
                _listener.Start();

                while (!_isDisposed)
                {
                    var socket = await _listener.AcceptSocketAsync();
                    var stream = new NetworkStream(socket, true);
                    await WireProtocol.Receive(stream, theReceiver);
                }
            });
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
            _isDisposed = true;
            _listener.Stop();
        }

        protected async Task afterSending()
        {
            using (var client = new TcpClient())
            {
                if (Dns.GetHostName() == destination.Host)
                {
                    await client.ConnectAsync(IPAddress.Loopback, destination.Port);
                }

                await client.ConnectAsync(destination.Host, destination.Port);

                await WireProtocol.Send(client.GetStream(), theMessageBatch, theSender);
            }
        }

        protected void allTheMessagesWereReceived()
        {
            theReceiver.MessagesReceived.Length.ShouldBe(theMessageBatch.Messages.Count);
            theReceiver.MessagesReceived.Select(x => x.Id)
                .ShouldHaveTheSameElementsAs(theMessageBatch.Messages.Select(x => x.Id));
        }



    }


    public class StubReceiverCallback : IReceiverCallback
    {
        public ReceivedStatus StatusToReturn;
        public bool ThrowErrorOnReceived;

        ReceivedStatus IReceiverCallback.Received(Message[] messages)
        {
            if (ThrowErrorOnReceived)
            {
                throw new DivideByZeroException();
            }

            MessagesReceived = messages;

            return StatusToReturn;

        }

        public Message[] MessagesReceived { get; set; }

        void IReceiverCallback.Acknowledged(Message[] messages)
        {
            WasAcknowledged = true;
        }

        public bool? WasAcknowledged { get; set; }

        void IReceiverCallback.NotAcknowledged(Message[] messages)
        {
            WasAcknowledged = false;
        }

        void IReceiverCallback.Failed(Exception exception, Message[] messages)
        {
            FailureException = exception;
        }

        public Exception FailureException { get; set; }
    }

    public class StubSenderCallback : ISenderCallback
    {
        public void Successful(OutgoingMessageBatch outgoing)
        {
            Succeeded = true;
        }

        public bool Succeeded { get; set; }

        void ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            TimedOut = true;
        }

        public bool TimedOut { get; set; }

        void ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            SerializationFailed = true;
        }

        public bool SerializationFailed { get; set; }

        void ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            QueueDoesNotExist = true;
        }

        public bool QueueDoesNotExist { get; set; }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            ProcessingFailed = true;
        }

        public bool ProcessingFailed { get; set; }
    }
}
