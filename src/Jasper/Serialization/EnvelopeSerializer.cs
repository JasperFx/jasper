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
        /// <summary>
        /// Read properties and header values from a dictionary into this envelope object
        /// </summary>
        /// <param name="dictionary"></param>
        public static void ReadPropertiesFromDictionary(IDictionary<string, object?> dictionary, Envelope? env)
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

        /// <summary>
        /// Read properties and header values from a dictionary into this envelope object
        /// </summary>
        /// <param name="dictionary"></param>
        public static void ReadPropertiesFromDictionary(IReadOnlyDictionary<string, string?> dictionary, Envelope? env)
        {
            foreach (var pair in dictionary)
            {
                readData(env, pair.Key, pair.Value);
            }

        }

        private static void readData(Envelope env, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case EnvelopeConstants.SourceKey:
                        env.Source = value;
                        break;

                    case EnvelopeConstants.MessageTypeKey:
                        env.MessageType = value;
                        break;

                    case EnvelopeConstants.ReplyUriKey:
                        env.ReplyUri = new Uri(value);
                        break;

                    case EnvelopeConstants.ContentTypeKey:
                        env.ContentType = value;
                        break;

                    case EnvelopeConstants.CorrelationIdKey:
                        env.CorrelationId = value;
                        break;

                    case EnvelopeConstants.SagaIdKey:
                        env.SagaId = value;
                        break;

                    case EnvelopeConstants.CausationIdKey:
                        env.CausationId = value;
                        break;

                    case EnvelopeConstants.DestinationKey:
                        env.Destination = new Uri(value);
                        break;

                    case EnvelopeConstants.AcceptedContentTypesKey:
                        env.AcceptedContentTypes = value.Split(',');
                        break;

                    case EnvelopeConstants.IdKey:
                        if (Guid.TryParse(value, out var id)) env.Id = id;
                        break;

                    case EnvelopeConstants.ReplyRequestedKey:
                        env.ReplyRequested = value;
                        break;

                    case EnvelopeConstants.AckRequestedKey:
                        env.AckRequested = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case EnvelopeConstants.ExecutionTimeKey:
                        env.ExecutionTime = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
                        break;

                    case EnvelopeConstants.AttemptsKey:
                        env.Attempts = Int32.Parse(value);
                        break;

                    case EnvelopeConstants.DeliverByHeader:
                        env.DeliverBy = DateTime.Parse(value);
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

        public static Envelope?[] ReadMany(byte[] buffer)
        {
            using var ms = new MemoryStream(buffer);
            using var br = new BinaryReader(ms);
            var numberOfMessages = br.ReadInt32();
            var msgs = new Envelope?[numberOfMessages];
            for (var i = 0; i < numberOfMessages; i++) msgs[i] = readSingle(br);
            return msgs;
        }

        internal static Envelope? Deserialize(byte[]? buffer)
        {
            using var ms = new MemoryStream(buffer);
            using var br = new BinaryReader(ms);
            return readSingle(br);
        }

        private static Envelope? readSingle(BinaryReader br)
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

        public static byte[] Serialize(IList<Envelope?> messages)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(messages.Count);
            foreach (var message in messages)
            {
                writeSingle(writer, message);
            }
            writer.Flush();
            return stream.ToArray();
        }

        public static byte[]? Serialize(Envelope? env)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writeSingle(writer, env);
            writer.Flush();
            return stream.ToArray();
        }

        private static void writeSingle(BinaryWriter writer, Envelope? env)
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
            dictionary.WriteProp(EnvelopeConstants.SourceKey, env.Source);
            dictionary.WriteProp(EnvelopeConstants.MessageTypeKey, env.MessageType);
            dictionary.WriteProp(EnvelopeConstants.ReplyUriKey, env.ReplyUri);
            dictionary.WriteProp(EnvelopeConstants.ContentTypeKey, env.ContentType);
            dictionary.WriteProp(EnvelopeConstants.CorrelationIdKey, env.CorrelationId);
            dictionary.WriteProp(EnvelopeConstants.CausationIdKey, env.CausationId);
            dictionary.WriteProp(EnvelopeConstants.DestinationKey, env.Destination);
            dictionary.WriteProp(EnvelopeConstants.SagaIdKey, env.SagaId);

            if (env.AcceptedContentTypes != null && env.AcceptedContentTypes.Any())
            {
                dictionary.WriteProp(EnvelopeConstants.AcceptedContentTypesKey, string.Join(",", env.AcceptedContentTypes));
            }

            dictionary.WriteProp(EnvelopeConstants.IdKey, env.Id);
            dictionary.WriteProp(EnvelopeConstants.ReplyRequestedKey, env.ReplyRequested);
            dictionary.WriteProp(EnvelopeConstants.AckRequestedKey, env.AckRequested);

            if (env.ExecutionTime.HasValue)
            {
                var dateString = env.ExecutionTime.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                dictionary.Add(EnvelopeConstants.ExecutionTimeKey, dateString);
            }


            dictionary.WriteProp(EnvelopeConstants.AttemptsKey, env.Attempts);
            dictionary.WriteProp(EnvelopeConstants.DeliverByHeader, env.DeliverBy);

            foreach (var pair in env.Headers)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        private static int writeHeaders(BinaryWriter writer, Envelope? env)
        {
            var count = 0;

            writer.WriteProp(ref count, EnvelopeConstants.SourceKey, env.Source);
            writer.WriteProp(ref count, EnvelopeConstants.MessageTypeKey, env.MessageType);
            writer.WriteProp(ref count, EnvelopeConstants.ReplyUriKey, env.ReplyUri);
            writer.WriteProp(ref count, EnvelopeConstants.ContentTypeKey, env.ContentType);
            writer.WriteProp(ref count, EnvelopeConstants.CorrelationIdKey, env.CorrelationId);
            writer.WriteProp(ref count, EnvelopeConstants.CausationIdKey, env.CausationId);
            writer.WriteProp(ref count, EnvelopeConstants.DestinationKey, env.Destination);
            writer.WriteProp(ref count, EnvelopeConstants.SagaIdKey, env.SagaId);

            if (env.AcceptedContentTypes != null && env.AcceptedContentTypes.Any())
            {
                writer.WriteProp(ref count, EnvelopeConstants.AcceptedContentTypesKey, string.Join(",", env.AcceptedContentTypes));
            }

            writer.WriteProp(ref count, EnvelopeConstants.IdKey, env.Id);
            writer.WriteProp(ref count, EnvelopeConstants.ReplyRequestedKey, env.ReplyRequested);
            writer.WriteProp(ref count, EnvelopeConstants.AckRequestedKey, env.AckRequested);

            if (env.ExecutionTime.HasValue)
            {
                var dateString = env.ExecutionTime.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                count++;
                writer.Write(EnvelopeConstants.ExecutionTimeKey);
                writer.Write(dateString);
            }


            writer.WriteProp(ref count, EnvelopeConstants.AttemptsKey, env.Attempts);
            writer.WriteProp(ref count, EnvelopeConstants.DeliverByHeader, env.DeliverBy);

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
