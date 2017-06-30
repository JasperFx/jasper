using System;
using System.Collections.Generic;
using System.IO;
using Jasper.Bus.Runtime;
using Jasper.Util;

namespace Jasper.Bus.Transports
{
    public static class SerializationExtensions
    {
        public static Envelope[] ToMessages(this byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                var numberOfMessages = br.ReadInt32();
                var msgs = new Envelope[numberOfMessages];
                for (int i = 0; i < numberOfMessages; i++)
                {
                    msgs[i] = readSingle<Envelope>(br);
                }
                return msgs;
            }
        }

        public static Envelope ToEnvelope(this byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                return readSingle<Envelope>(br);
            }
        }

        private static TMessage readSingle<TMessage>(BinaryReader br) where TMessage : Envelope, new()
        {
            var msg = new TMessage
            {
                Id = new MessageId
                {
                    SourceInstanceId = new Guid(br.ReadBytes(16)),
                    MessageIdentifier = new Guid(br.ReadBytes(16))
                },
                Queue = br.ReadString(),
                SubQueue = br.ReadString(),
                SentAt = DateTime.FromBinary(br.ReadInt64()),
            };
            var headerCount = br.ReadInt32();
            msg.Headers = new Dictionary<string, string>();
            for (var j = 0; j < headerCount; j++)
            {
                msg.Headers.Add(
                    br.ReadString(),
                    br.ReadString()
                    );
            }
            var byteCount = br.ReadInt32();
            msg.Data = br.ReadBytes(byteCount);
            if (string.IsNullOrEmpty(msg.SubQueue))
                msg.SubQueue = null;

            return msg;
        }

        public static byte[] Serialize(this IList<Envelope> messages)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(messages.Count);
                foreach (var message in messages)
                {
                    writeSingle(message, writer);
                }
                writer.Flush();
                return stream.ToArray();
            }
        }

        public static byte[] Serialize(this Envelope message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writeSingle(message, writer);
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static void writeSingle(Envelope message, BinaryWriter writer)
        {
            writer.Write(message.Id.SourceInstanceId.ToByteArray());
            writer.Write(message.Id.MessageIdentifier.ToByteArray());
            writer.Write(message.Destination.QueueName());
            writer.Write(message.SubQueue ?? "");
            writer.Write(message.SentAt.ToBinary());

            writer.Write(message.Headers.Count);
            foreach (var pair in message.Headers)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            writer.Write(message.Data.Length);
            writer.Write(message.Data);
        }
    }
}
