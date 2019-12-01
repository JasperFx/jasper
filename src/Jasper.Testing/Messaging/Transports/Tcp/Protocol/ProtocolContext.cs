using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Tcp.Protocol
{
    [Collection("protocol")]
    public abstract class ProtocolContext : IDisposable
    {
        protected static int NextPort = 6005;
        private readonly IPAddress theAddress = IPAddress.Loopback;
        private readonly int thePort = ++NextPort;
        private readonly ListeningAgent _listener;
        private readonly Uri destination;
        private readonly OutgoingMessageBatch theMessageBatch;


        protected StubReceiverCallback theReceiver = new StubReceiverCallback();
        protected StubSenderCallback theSender = new StubSenderCallback();

        public ProtocolContext()
        {
            destination = $"durable://localhost:{thePort}/incoming".ToUri();
            _listener = new ListeningAgent(theReceiver, theAddress, thePort, "durable", CancellationToken.None);


            var messages = new[]
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

        public void Dispose()
        {
            _listener.Dispose();
        }


        private Envelope outgoingMessage()
        {
            return new Envelope
            {
                Destination = destination,
                Data = new byte[] {1, 2, 3, 4, 5, 6, 7},
                SentAt = DateTime.Today.ToUniversalTime()
            };
        }

        protected async Task afterSending()
        {
            _listener.Start();

            using (var client = new TcpClient())
            {
                if (Dns.GetHostName() == destination.Host)
                    await client.ConnectAsync(IPAddress.Loopback, destination.Port);

                await client.ConnectAsync(destination.Host, destination.Port);

                await WireProtocol.Send(client.GetStream(), theMessageBatch, null, theSender);
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

        public Envelope[] MessagesReceived { get; set; }

        public bool? WasAcknowledged { get; set; }

        public Exception FailureException { get; set; }

        Task<ReceivedStatus> IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            if (ThrowErrorOnReceived) throw new DivideByZeroException();

            MessagesReceived = messages;

            return Task.FromResult(StatusToReturn);
        }

        Task IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            WasAcknowledged = true;
            return Task.CompletedTask;
        }

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
    }

    public class StubSenderCallback : ISenderCallback
    {
        public bool Succeeded { get; set; }

        public bool TimedOut { get; set; }

        public bool SerializationFailed { get; set; }

        public bool QueueDoesNotExist { get; set; }

        public bool ProcessingFailed { get; set; }

        public Task Successful(OutgoingMessageBatch outgoing)
        {
            Succeeded = true;
            return Task.CompletedTask;
        }

        public Task Successful(Envelope outgoing)
        {
            Succeeded = true;
            return Task.CompletedTask;
        }

        Task ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            TimedOut = true;
            return Task.CompletedTask;
        }

        Task ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            SerializationFailed = true;
            return Task.CompletedTask;
        }

        Task ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            QueueDoesNotExist = true;
            return Task.CompletedTask;
        }

        Task ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            ProcessingFailed = true;
            return Task.CompletedTask;
        }

        public Task ProcessingFailure(Envelope outgoing, Exception exception)
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

        public Task StopSending()
        {
            throw new NotImplementedException();
        }
    }
}
