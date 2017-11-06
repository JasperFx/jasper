using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Util;

namespace Jasper.Bus.Transports.Tcp
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
        public static async Task Send(Stream stream, OutgoingMessageBatch batch, byte[] messageBytes, ISenderCallback callback)
        {
            messageBytes = messageBytes ?? Envelope.Serialize(batch.Messages);

            var lengthBytes = BitConverter.GetBytes(messageBytes.Length);


            await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            // All four of the possible receive confirmation messages are the same length: 8 characters long encoded in UTF-16.
            var confirmationBytes = await stream.ReadBytesAsync(ReceivedBuffer.Length).ConfigureAwait(false);
            if (confirmationBytes.SequenceEqual(ReceivedBuffer))
            {
                callback.Successful(batch);

                await stream.WriteAsync(AcknowledgedBuffer, 0, AcknowledgedBuffer.Length);
            }
            else if (confirmationBytes.SequenceEqual(ProcessingFailureBuffer))
            {
                callback.ProcessingFailure(batch);
            }
            else if (confirmationBytes.SequenceEqual(SerializationFailureBuffer))
            {
                callback.SerializationFailure(batch);
            }
            else if (confirmationBytes.SequenceEqual(QueueDoesNotExistBuffer))
            {
                callback.QueueDoesNotExist(batch);
            }
        }


        public static async Task Receive(Stream stream, IReceiverCallback callback, Uri uri)
        {
            Envelope[] messages = null;

            try
            {
                var lengthBytes = await stream.ReadBytesAsync(sizeof(int));
                var length = BitConverter.ToInt32(lengthBytes, 0);
                if (length == 0) return;

                var bytes = await stream.ReadBytesAsync(length);
                messages = Envelope.ReadMany(bytes);


            }
            catch (Exception e)
            {
                callback.Failed(e, messages);
                await stream.SendBuffer(SerializationFailureBuffer);
                return;
            }

            try
            {
                await receive(stream, callback, messages, uri);
            }
            catch(Exception ex)
            {
                callback.Failed(ex, messages);
                await stream.SendBuffer(ProcessingFailureBuffer);
            }
        }

        private static async Task receive(Stream stream, IReceiverCallback callback, Envelope[] messages, Uri uri)
        {
            var status = await callback.Received(uri, messages);
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

                    var ack = await stream.ReadExpectedBuffer(AcknowledgedBuffer);

                    if (ack)
                    {
                        callback.Acknowledged(messages);
                    }
                    else
                    {
                        callback.NotAcknowledged(messages);
                    }
                    break;
            }
        }
    }
}
