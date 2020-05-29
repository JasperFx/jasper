using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jasper.Transports.Util;

namespace Jasper.Transports.Tcp
{
    public static class WireProtocol
    {
        // The first four values are the possible receive confirmation messages.
        // NOTE: "Recieved" is misspelled intentionally to preserve compatibility.
        // The original Rhino Queues protocol used this spelling: https://github.com/hibernating-rhinos/rhino-queues/blob/master/Rhino.Queues/Protocol/ProtocolConstants.cs
        public const string Received = "Recieved";
        public const string SerializationFailure = "FailDesr";
        public const string ProcessingFailure = "FailPrcs";
        public const string QueueDoesNotExist = "Qu-Exist";

        public const string Acknowledged = "Acknowledged";
        public const string Revert = "Revert";

        public static byte[] ReceivedBuffer = Encoding.Unicode.GetBytes(Received);
        public static byte[] SerializationFailureBuffer = Encoding.Unicode.GetBytes(SerializationFailure);
        public static byte[] ProcessingFailureBuffer = Encoding.Unicode.GetBytes(ProcessingFailure);
        public static byte[] QueueDoesNotExistBuffer = Encoding.Unicode.GetBytes(QueueDoesNotExist);

        public static byte[] AcknowledgedBuffer = Encoding.Unicode.GetBytes(Acknowledged);
        public static byte[] RevertBuffer = Encoding.Unicode.GetBytes(Revert);

        // Nothing but actually sending here. Worry about timeouts and retries somewhere
        // else
        public static async Task<SendStatus> Send(Stream stream, OutgoingMessageBatch batch, byte[] messageBytes)
        {
            messageBytes ??= Envelope.Serialize(batch.Messages);

            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            // All four of the possible receive confirmation messages are the same length: 8 characters long encoded in UTF-16.
            byte[] confirmationBytes = await stream.ReadBytesAsync(ReceivedBuffer.Length).ConfigureAwait(false);
            if (confirmationBytes.SequenceEqual(ReceivedBuffer))
            {
                return SendStatus.Success;
            }
            else if (confirmationBytes.SequenceEqual(ProcessingFailureBuffer))
            {
                return SendStatus.Failure;
            }
            else if (confirmationBytes.SequenceEqual(SerializationFailureBuffer))
            {
                return SendStatus.SerializationFailure;
            }
            else if (confirmationBytes.SequenceEqual(QueueDoesNotExistBuffer))
            {
                return SendStatus.QueueDoesNotExist;
            }

            return SendStatus.Failure;
        }

        public static Task Ack(Stream stream)  => stream.WriteAsync(AcknowledgedBuffer, 0, AcknowledgedBuffer.Length);

        public enum SendStatus
        {
            Failure,
            Success,
            SerializationFailure,
            QueueDoesNotExist
        }

        public class BeginReceiveResult
        {
            public ReceivedStatus Status { get; }
            public Envelope[] Messages { get; }
            public Exception Exception { get; }

            BeginReceiveResult(ReceivedStatus status)
            {
                Status = status;
            }

            public BeginReceiveResult(ReceivedStatus status, Envelope[] messages) : this(status)
            {
                Messages = messages;
            }

            public BeginReceiveResult(ReceivedStatus status, Exception exception) : this(status)
            {
                Exception = exception;
            }
        }

        public static async Task<BeginReceiveResult> BeginReceive(Stream stream, Uri uri)
        {
            try
            {
                var lengthBytes = await stream.ReadBytesAsync(sizeof(int));
                var length = BitConverter.ToInt32(lengthBytes, 0);
                if (length == 0) return new BeginReceiveResult(ReceivedStatus.Successful, new Envelope[0]);

                var bytes = await stream.ReadBytesAsync(length);
                var messages = Envelope.ReadMany(bytes);
                return new BeginReceiveResult(ReceivedStatus.Successful, messages);
            }
            catch (Exception e)
            {
                return new BeginReceiveResult(ReceivedStatus.SerializationFailure, e);
            }
        }

        public static async Task<ReceivedStatus> EndReceive(Stream stream, ReceivedStatus status)
        {
            switch (status)
            {
                case ReceivedStatus.ProcessFailure:
                    await stream.SendBuffer(ProcessingFailureBuffer);
                    break;


                case ReceivedStatus.QueueDoesNotExist:
                    await stream.SendBuffer(QueueDoesNotExistBuffer);
                    break;

                default:
                    await stream.SendBuffer(ReceivedBuffer);

                    bool ack = await stream.ReadExpectedBuffer(AcknowledgedBuffer);

                    return ack ? ReceivedStatus.Acknowledged : ReceivedStatus.NotAcknowledged;
            }

            return status;
        }
    }
}
