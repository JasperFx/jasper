using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Jasper.Util;

namespace Jasper.Serialization
{
    public class EnvelopeSerializer
    {
        public const string CorrelationIdKey = "correlation-id";
        public const string SagaIdKey = "saga-id";
        private const string IdKey = "id";
        public const string CausationIdKey = "parent-id";
        private const string ContentTypeKey = "content-type";
        public const string SourceKey = "source";
        public const string ReplyRequestedKey = "reply-requested";
        public const string DestinationKey = "destination";
        public const string ReplyUriKey = "reply-uri";
        public const string ExecutionTimeKey = "time-to-send";
        public const string ReceivedAtKey = "received-at";
        public const string AttemptsKey = "attempts";
        public const string AckRequestedKey = "ack-requested";
        public const string MessageTypeKey = "message-type";
        public const string AcceptedContentTypesKey = "accepted-content-types";
        public const string DeliverByHeader = "deliver-by";

        /// <summary>
        /// Read properties and header values from a dictionary into this envelope object
        /// </summary>
        /// <param name="dictionary"></param>
        public static void ReadPropertiesFromDictionary(IDictionary<string, object> dictionary, Envelope env)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Value is byte[] data)
                {
                    var raw = Encoding.Default.GetString(data);
                    readData(env, pair.Key, raw);
                }
                else if (pair.Value is string value)
                {
                    readData(env, pair.Key, value);
                }
            }

        }

        private static void readData(Envelope env, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case SourceKey:
                        env.Source = value;
                        break;

                    case MessageTypeKey:
                        env.MessageType = value;
                        break;

                    case ReplyUriKey:
                        env.ReplyUri = new Uri(value);
                        break;

                    case ContentTypeKey:
                        env.ContentType = value;
                        break;

                    case CorrelationIdKey:
                        if (Guid.TryParse(value, out var originalId)) env.CorrelationId = originalId;
                        break;

                    case SagaIdKey:
                        env.SagaId = value;
                        break;

                    case CausationIdKey:
                        if (Guid.TryParse(value, out var parentId)) env.CausationId = parentId;
                        break;

                    case DestinationKey:
                        env.Destination = new Uri(value);
                        break;

                    case AcceptedContentTypesKey:
                        env.AcceptedContentTypes = value.Split(',');
                        break;

                    case IdKey:
                        if (Guid.TryParse(value, out var id)) env.Id = id;
                        break;

                    case ReplyRequestedKey:
                        env.ReplyRequested = value;
                        break;

                    case AckRequestedKey:
                        env.AckRequested = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case ExecutionTimeKey:
                        env.ExecutionTime = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
                        break;

                    case AttemptsKey:
                        env.Attempts = Int32.Parse(value);
                        break;

                    case DeliverByHeader:
                        env.DeliverBy = DateTime.Parse(value);
                        break;

                    case ReceivedAtKey:
                        env.ReceivedAt = new Uri(value);
                        break;


                    default:
                        env.Headers.Add(key, value);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error trying to read data for {key} = '{value}'", e);
            }
        }

        internal static Envelope[] ReadMany(byte[] buffer)
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

        internal static Envelope Deserialize(byte[] buffer)
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

            for (var j = 0; j < headerCount; j++)
            {
                readData(msg, br.ReadString(), br.ReadString());
            }

            var byteCount = br.ReadInt32();
            msg.Data = br.ReadBytes(byteCount);

            return msg;
        }

        internal static byte[] Serialize(IList<Envelope> messages)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(messages.Count);
                foreach (var message in messages)
                {
                    writeSingle(writer, message);
                }
                writer.Flush();
                return stream.ToArray();
            }
        }

        internal static byte[] Serialize(Envelope env)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writeSingle(writer, env);
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static void writeSingle(BinaryWriter writer, Envelope env)
        {
            writer.Write(env.SentAt.ToBinary());

            writer.Flush();

            using (var headerData = new MemoryStream())
            {
                using (var headerWriter = new BinaryWriter(headerData))
                {
                    var count = writeHeaders(headerWriter, env);
                    headerWriter.Flush();

                    writer.Write(count);

                    headerData.Position = 0;
                    headerData.CopyTo(writer.BaseStream);
                }
            }

            writer.Write(env.Data.Length);
            writer.Write(env.Data);
        }


        /// <summary>
        /// Write properties of an Envelope to a Dictionary for transmission
        /// </summary>
        /// <param name="dictionary"></param>
        public static void WriteToDictionary(IDictionary<string, object> dictionary, Envelope env)
        {
            dictionary.WriteProp(SourceKey, env.Source);
            dictionary.WriteProp(MessageTypeKey, env.MessageType);
            dictionary.WriteProp(ReplyUriKey, env.ReplyUri);
            dictionary.WriteProp(ContentTypeKey, env.ContentType);
            dictionary.WriteProp(CorrelationIdKey, env.CorrelationId);
            dictionary.WriteProp(CausationIdKey, env.CausationId);
            dictionary.WriteProp(DestinationKey, env.Destination);
            dictionary.WriteProp(SagaIdKey, env.SagaId);

            if (env.AcceptedContentTypes != null && env.AcceptedContentTypes.Any())
            {
                dictionary.WriteProp(AcceptedContentTypesKey, string.Join(",", env.AcceptedContentTypes));
            }

            dictionary.WriteProp(IdKey, env.Id);
            dictionary.WriteProp(ReplyRequestedKey, env.ReplyRequested);
            dictionary.WriteProp(AckRequestedKey, env.AckRequested);

            if (env.ExecutionTime.HasValue)
            {
                var dateString = env.ExecutionTime.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                dictionary.Add(ExecutionTimeKey, dateString);
            }


            dictionary.WriteProp(AttemptsKey, env.Attempts);
            dictionary.WriteProp(DeliverByHeader, env.DeliverBy);
            dictionary.WriteProp(ReceivedAtKey, env.ReceivedAt);

            foreach (var pair in env.Headers)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        private static int writeHeaders(BinaryWriter writer, Envelope env)
        {
            var count = 0;

            writer.WriteProp(ref count, SourceKey, env.Source);
            writer.WriteProp(ref count, MessageTypeKey, env.MessageType);
            writer.WriteProp(ref count, ReplyUriKey, env.ReplyUri);
            writer.WriteProp(ref count, ContentTypeKey, env.ContentType);
            writer.WriteProp(ref count, CorrelationIdKey, env.CorrelationId);
            writer.WriteProp(ref count, CausationIdKey, env.CausationId);
            writer.WriteProp(ref count, DestinationKey, env.Destination);
            writer.WriteProp(ref count, SagaIdKey, env.SagaId);

            if (env.AcceptedContentTypes != null && env.AcceptedContentTypes.Any())
            {
                writer.WriteProp(ref count, AcceptedContentTypesKey, string.Join(",", env.AcceptedContentTypes));
            }

            writer.WriteProp(ref count, IdKey, env.Id);
            writer.WriteProp(ref count, ReplyRequestedKey, env.ReplyRequested);
            writer.WriteProp(ref count, AckRequestedKey, env.AckRequested);

            if (env.ExecutionTime.HasValue)
            {
                var dateString = env.ExecutionTime.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                count++;
                writer.Write(ExecutionTimeKey);
                writer.Write(dateString);
            }


            writer.WriteProp(ref count, AttemptsKey, env.Attempts);
            writer.WriteProp(ref count, DeliverByHeader, env.DeliverBy);
            writer.WriteProp(ref count, ReceivedAtKey, env.ReceivedAt);

            foreach (var pair in env.Headers)
            {
                count++;
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }


            return count;
        }
    }
}
