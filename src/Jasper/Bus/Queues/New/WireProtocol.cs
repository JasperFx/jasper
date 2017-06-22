using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Queues.Net;
using Jasper.Bus.Queues.Net.Protocol;
using Jasper.Bus.Queues.Net.Protocol.V1;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Queues.Storage;

namespace Jasper.Bus.Queues.New
{
    public static class WireProtocol
    {
        // Nothing but actually sending here. Worry about timeouts and retries somewhere
        // else

        // TODO -- need a generic error here, or just let it bubble up? Cool w/ that TBH
        public static async Task Send(Stream stream, OutgoingMessageBatch batch, ISenderCallback callback)
        {
            var messageBytes = batch.Messages.Serialize();
            var lengthBytes = BitConverter.GetBytes(messageBytes.Length);


            await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            var bytes = await stream.ReadBytesAsync(Constants.ReceivedBuffer.Length).ConfigureAwait(false);
            if (bytes.SequenceEqual(Constants.ReceivedBuffer))
            {
                callback.Successful(batch);

                await stream.WriteAsync(Constants.AcknowledgedBuffer, 0, Constants.AcknowledgedBuffer.Length);
            }
            else if (bytes.SequenceEqual(Constants.SerializationFailureBuffer))
            {
                callback.SerializationFailure(batch);
            }
            else if (bytes.SequenceEqual(Constants.QueueDoesNotExistBuffer))
            {
                callback.QueueDoesNotExist(batch);
            }
        }


        public static async Task Receive(Stream stream, IReceiverCallback callback)
        {
            Message[] messages = null;

            try
            {
                var lengthBytes = await stream.ReadBytesAsync(sizeof(int));
                var length = BitConverter.ToInt32(lengthBytes, 0);
                if (length == 0) return;

                var bytes = await stream.ReadBytesAsync(length);
                messages = bytes.ToMessages();


            }
            catch (Exception e)
            {
                callback.Failed(e, messages);
                await stream.SendBuffer(Constants.SerializationFailureBuffer);
                return;
            }

            try
            {
                await receive(stream, callback, messages);
            }
            catch(Exception ex)
            {
                callback.Failed(ex, messages);
                await stream.SendBuffer(Constants.ProcessingFailureBuffer);
            }
        }

        private static async Task receive(Stream stream, IReceiverCallback callback, Message[] messages)
        {
            var status = callback.Received(messages);
            switch (status)
            {
                case ReceivedStatus.ProcessFailure:
                    await stream.SendBuffer(Constants.ProcessingFailureBuffer);
                    break;


                case ReceivedStatus.QueueDoesNotExist:
                    await stream.SendBuffer(Constants.QueueDoesNotExistBuffer);
                    break;

                default:
                    await stream.SendBuffer(Constants.ReceivedBuffer);

                    var ack = await stream.ReadExpectedBuffer(Constants.AcknowledgedBuffer);

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

    public static class StreamExtensions
    {
        public static Task SendBuffer(this Stream stream, byte[] buffer)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public interface ISenderCallback
    {
        void Successful(OutgoingMessageBatch outgoing);
        void TimedOut(OutgoingMessageBatch outgoing);
        void SerializationFailure(OutgoingMessageBatch outgoing);
        void QueueDoesNotExist(OutgoingMessageBatch outgoing);
        void ProcessingFailure(OutgoingMessageBatch outgoing);
    }

    public interface IReceiverCallback
    {
        ReceivedStatus Received(Message[] messages);
        void Acknowledged(Message[] messages);
        void NotAcknowledged(Message[] messages);
        void Failed(Exception exception, Message[] messages);
    }

    public enum ReceivedStatus
    {
        Successful,
        QueueDoesNotExist,
        ProcessFailure
    }
}
