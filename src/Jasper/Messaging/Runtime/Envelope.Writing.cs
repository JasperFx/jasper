using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


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
                for (var i = 0; i < numberOfMessages; i++) msgs[i] = readSingle(br);
                return msgs;
            }
        }

        public static Envelope Deserialize(byte[] buffer)
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
                SentAt = DateTime.FromBinary(br.ReadInt64())
            };
            var headerCount = br.ReadInt32();

            for (var j = 0; j < headerCount; j++) msg.ReadData(br.ReadString(), br.ReadString());

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
                foreach (var message in messages) message.writeSingle(writer);
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

            EnsureData();

            writer.Write(Data.Length);
            writer.Write(Data);
        }


        public void WriteToDictionary(IDictionary<string, object> dictionary)
        {
            dictionary.WriteProp(SourceKey, Source);
            dictionary.WriteProp(MessageTypeKey, MessageType);
            dictionary.WriteProp(ReplyUriKey, ReplyUri);
            dictionary.WriteProp(ContentTypeKey, ContentType);
            dictionary.WriteProp(CorrelationIdKey, CorrelationId);
            dictionary.WriteProp(ParentIdKey, CausationId);
            dictionary.WriteProp(DestinationKey, Destination);
            dictionary.WriteProp(SagaIdKey, SagaId);

            if (AcceptedContentTypes != null && AcceptedContentTypes.Any())
                dictionary.WriteProp(AcceptedContentTypesKey, string.Join(",", AcceptedContentTypes));

            dictionary.WriteProp(IdKey, Id);
            dictionary.WriteProp(ReplyRequestedKey, ReplyRequested);
            dictionary.WriteProp(AckRequestedKey, AckRequested);

            if (ExecutionTime.HasValue)
            {
                var dateString = ExecutionTime.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                dictionary.Add(ExecutionTimeKey, dateString);
            }


            dictionary.WriteProp(AttemptsKey, Attempts);
            dictionary.WriteProp(DeliverByHeader, DeliverBy);
            dictionary.WriteProp(SentAttemptsHeaderKey, SentAttempts);
            dictionary.WriteProp(ReceivedAtKey, ReceivedAt);

            foreach (var pair in Headers) dictionary.Add(pair.Key, pair.Value);
        }

        private int writeHeaders(BinaryWriter writer)
        {
            var count = 0;

            writer.WriteProp(ref count, SourceKey, Source);
            writer.WriteProp(ref count, MessageTypeKey, MessageType);
            writer.WriteProp(ref count, ReplyUriKey, ReplyUri);
            writer.WriteProp(ref count, ContentTypeKey, ContentType);
            writer.WriteProp(ref count, CorrelationIdKey, CorrelationId);
            writer.WriteProp(ref count, ParentIdKey, CausationId);
            writer.WriteProp(ref count, DestinationKey, Destination);
            writer.WriteProp(ref count, SagaIdKey, SagaId);

            if (AcceptedContentTypes != null && AcceptedContentTypes.Any())
                writer.WriteProp(ref count, AcceptedContentTypesKey, string.Join(",", AcceptedContentTypes));

            writer.WriteProp(ref count, IdKey, Id);
            writer.WriteProp(ref count, ReplyRequestedKey, ReplyRequested);
            writer.WriteProp(ref count, AckRequestedKey, AckRequested);

            if (ExecutionTime.HasValue)
            {
                var dateString = ExecutionTime.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
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
