using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Lightweight.Protocol
{
    public abstract class ProtocolContext : IDisposable
    {
        protected static int NextPort = 6000;


        protected StubReceiverCallback theReceiver = new StubReceiverCallback();
        protected StubSenderCallback theSender = new StubSenderCallback();
        private readonly IPAddress theAddress = IPAddress.Loopback;
        private readonly int thePort = ++NextPort;
        private Uri destination;
        private OutgoingMessageBatch theMessageBatch;
        private ListeningAgent _listener;

        public ProtocolContext()
        {
            destination = $"durable://localhost:{thePort}/incoming".ToUri();
            _listener = new ListeningAgent(theReceiver, theAddress, thePort, "durable", CancellationToken.None);



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
                Destination = destination,
                Data = new byte[]{1,2,3,4,5,6,7},
                SentAt = DateTime.Today.ToUniversalTime()
            };
        }

        public void Dispose()
        {
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

                await WireProtocol.Send(client.GetStream(), theMessageBatch, null, theSender);
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

        Task<ReceivedStatus> IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            if (ThrowErrorOnReceived)
            {
                throw new DivideByZeroException();
            }

            MessagesReceived = messages;

            return Task.FromResult(StatusToReturn);

        }

        public Envelope[] MessagesReceived { get; set; }

        Task IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            WasAcknowledged = true;
            return Task.CompletedTask;
        }

        public bool? WasAcknowledged { get; set; }

        Task IReceiverCallback.NotAcknowledged(Envelope[] messages)
        {
            WasAcknowledged = false;
            return Task.CompletedTask;
        }

        Task IReceiverCallback.Failed(Exception exception, Envelope[] messages)
        {
            FailureException = exception;
            return Task.CompletedTask;
        }

        public Exception FailureException { get; set; }
    }

    public class StubSenderCallback : ISenderCallback
    {
        public Task Successful(OutgoingMessageBatch outgoing)
        {
            Succeeded = true;
            return Task.CompletedTask;
        }

        public bool Succeeded { get; set; }

        Task ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            TimedOut = true;
            return Task.CompletedTask;
        }

        public bool TimedOut { get; set; }

        Task ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            SerializationFailed = true;
            return Task.CompletedTask;
        }

        public bool SerializationFailed { get; set; }

        Task ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            QueueDoesNotExist = true;
            return Task.CompletedTask;
        }

        public bool QueueDoesNotExist { get; set; }

        Task ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            ProcessingFailed = true;
            return Task.CompletedTask;
        }

        public Task ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task SenderIsLatched(OutgoingMessageBatch outgoing)
        {
            throw new NotImplementedException();
        }

        public bool ProcessingFailed { get; set; }
    }
}
