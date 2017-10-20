using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;
using Jasper.Util;
using Shouldly;

namespace Jasper.Testing.Bus.Lightweight.Protocol
{
    public abstract class ProtocolContext : IDisposable
    {
        protected StubReceiverCallback theReceiver = new StubReceiverCallback();
        protected StubSenderCallback theSender = new StubSenderCallback();
        private readonly IPAddress theAddress = IPAddress.Loopback;
        private readonly int thePort = 2112;
        private Uri destination = $"durable://localhost:2112/incoming".ToUri();
        private OutgoingMessageBatch theMessageBatch;
        private bool _isDisposed;
        private ListeningAgent _listener;

        public ProtocolContext()
        {
            _listener = new ListeningAgent(theReceiver, thePort, "durable", CancellationToken.None);



            var messages = new Envelope[]
            {
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage(),
                outgoingMessage()
            };

            theMessageBatch = new OutgoingMessageBatch(destination, messages);

        }


        private Envelope outgoingMessage()
        {
            return new Envelope
            {
                EnvelopeVersionId = MessageId.GenerateRandom(),
                Destination = destination,
                Data = new byte[]{1,2,3,4,5,6,7},
                Queue = "outgoing",
                SentAt = DateTime.Today.ToUniversalTime()
            };
        }

        public void Dispose()
        {
            _isDisposed = true;
            _listener.Dispose();
        }

        protected async Task afterSending()
        {
            _listener.Start();

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
            Testing.SpecificationExtensions.ShouldHaveTheSameElementsAs(theReceiver.MessagesReceived.Select(x => x.EnvelopeVersionId), theMessageBatch.Messages.Select(x => x.EnvelopeVersionId));
        }



    }


    public class StubReceiverCallback : IReceiverCallback
    {
        public ReceivedStatus StatusToReturn;
        public bool ThrowErrorOnReceived;

        ReceivedStatus IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            if (ThrowErrorOnReceived)
            {
                throw new DivideByZeroException();
            }

            MessagesReceived = messages;

            return StatusToReturn;

        }

        public Envelope[] MessagesReceived { get; set; }

        void IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            WasAcknowledged = true;
        }

        public bool? WasAcknowledged { get; set; }

        void IReceiverCallback.NotAcknowledged(Envelope[] messages)
        {
            WasAcknowledged = false;
        }

        void IReceiverCallback.Failed(Exception exception, Envelope[] messages)
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

        public void ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            throw new NotImplementedException();
        }

        public bool ProcessingFailed { get; set; }
    }
}
