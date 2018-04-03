using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Messaging.Runtime
{
    public partial class Envelope
    {
        public static Envelope[] ReadMany(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                var numberOfMessages = br.ReadInt32();
                var msgs = new Envelope[numberOfMessages];
                for (int i = 0; i < numberOfMessages; i++)
                {
                    msgs[i] = readSingle(br);
                }
                return msgs;
            }
        }

        public static Envelope Read(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                return readSingle(br);
            }
        }

        private static Envelope readSingle(BinaryReader br)
        {
            var msg = new Envelope
            {
                EnvelopeVersionId = new PersistedMessageId
                {
                    SourceInstanceId = new Guid(br.ReadBytes(16)),
                    MessageIdentifier = new Guid(br.ReadBytes(16))
                },
                Queue = br.ReadString(),
                SubQueue = br.ReadString(),
                SentAt = DateTime.FromBinary(br.ReadInt64()),
            };
            var headerCount = br.ReadInt32();

            for (var j = 0; j < headerCount; j++)
            {
                msg.ReadData(br.ReadString(), br.ReadString());
            }

            var byteCount = br.ReadInt32();
            msg.Data = br.ReadBytes(byteCount);

            return msg;
        }

        public static byte[] Serialize(IList<Envelope> messages)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(messages.Count);
                foreach (var message in messages)
                {
                    message.writeSingle(writer);
                }
                writer.Flush();
                return stream.ToArray();
            }
        }

        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writeSingle(writer);
                writer.Flush();
                return stream.ToArray();
            }
        }

        // TODO -- should we be using some kind of memory pooling here?
        private void writeSingle(BinaryWriter writer)
        {
            writer.Write(EnvelopeVersionId.SourceInstanceId.ToByteArray());
            writer.Write(EnvelopeVersionId.MessageIdentifier.ToByteArray());
            writer.Write(Destination?.QueueName() ?? "");
            writer.Write(SubQueue ?? "");
            writer.Write(SentAt.ToBinary());

            writer.Flush();

            using (var headerData = new MemoryStream())
            {
                using (var headerWriter = new BinaryWriter(headerData))
                {
                    var count = writeHeaders(headerWriter);
                    headerWriter.Flush();

                    writer.Write(count);

                    headerData.Position = 0;
                    headerData.CopyTo(writer.BaseStream);
                }

            }

            writer.Write(Data.Length);
            writer.Write(Data);

        }

        private int writeHeaders(BinaryWriter writer)
        {
            int count = 0;

            writer.WriteProp(ref count, SourceKey, Source);
            writer.WriteProp(ref count, MessageTypeKey, MessageType);
            writer.WriteProp(ref count, ReplyUriKey, ReplyUri);
            writer.WriteProp(ref count, ContentTypeKey, ContentType);
            writer.WriteProp(ref count, OriginalIdKey, OriginalId);
            writer.WriteProp(ref count, ParentIdKey, ParentId);
            writer.WriteProp(ref count, ResponseIdKey, ResponseId);
            writer.WriteProp(ref count, DestinationKey, Destination);
            writer.WriteProp(ref count, SagaIdKey, SagaId);

            if (AcceptedContentTypes != null && AcceptedContentTypes.Any())
            {
                writer.WriteProp(ref count, AcceptedContentTypesKey, AcceptedContentTypes.Join(","));
            }

            writer.WriteProp(ref count, IdKey, Id);
            writer.WriteProp(ref count, ReplyRequestedKey, ReplyRequested);
            writer.WriteProp(ref count, AckRequestedKey, AckRequested);

            if (ExecutionTime.HasValue)
            {
                var dateString = ExecutionTime.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                count++;
                writer.Write(ExecutionTimeKey);
                writer.Write(dateString);
            }


            writer.WriteProp(ref count, AttemptsKey, Attempts);
            writer.WriteProp(ref count, DeliverByHeader, DeliverBy);
            writer.WriteProp(ref count, SentAttemptsHeaderKey, SentAttempts);
            writer.WriteProp(ref count, ReceivedAtKey, ReceivedAt);


            foreach (var pair in Headers)
            {
                count++;
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }


            return count;
        }

    }

    internal static class BinaryWriterExtensions
    {
        public static void WriteProp(this BinaryWriter writer, ref int count, string key, string value)
        {
            if (value == null) return;

            writer.Write(key);
            writer.Write(value);

            count++;
        }

        public static void WriteProp(this BinaryWriter writer, ref int count, string key, int value)
        {
            if (value > 0)
            {
                writer.Write(key);
                writer.Write(value.ToString());

                count++;
            }


        }

        public static void WriteProp(this BinaryWriter writer, ref int count, string key, Guid value)
        {
            if (value != Guid.Empty)
            {
                writer.Write(key);
                writer.Write(value.ToString());

                count++;
            }


        }

        public static void WriteProp(this BinaryWriter writer, ref int count, string key, bool value)
        {
            if (value)
            {
                writer.Write(key);
                writer.Write(value.ToString());

                count++;
            }


        }

        public static void WriteProp(this BinaryWriter writer, ref int count, string key, DateTime? value)
        {
            if (value.HasValue)
            {
                writer.Write(key);
                writer.Write(value.Value.ToString("o"));

                count++;
            }


        }

        public static void WriteProp(this BinaryWriter writer, ref int count, string key, DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                writer.Write(key);
                writer.Write(value.Value.ToString("o"));

                count++;
            }


        }

        public static void WriteProp(this BinaryWriter writer, ref int count, string key, Uri value)
        {
            if (value == null) return;

            writer.Write(key);
            writer.Write(value.ToString());

            count++;
        }
    }
}
