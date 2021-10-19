using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Jasper
{
    public partial class Envelope
    {
        /// <summary>
        /// Read properties and header values from a dictionary into this envelope object
        /// </summary>
        /// <param name="dictionary"></param>
        public void ReadPropertiesFromDictionary(IDictionary<string, object> dictionary)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Value is byte[] data)
                {
                    var raw = Encoding.Default.GetString(data);
                    readData(pair.Key, raw);
                }
                else if (pair.Value is string value)
                {
                    readData(pair.Key, value);
                }
            }
        }

        private void readData(string key, string value)
        {
            try
            {
                switch (key)
                {
                    case SourceKey:
                        Source = value;
                        break;

                    case MessageTypeKey:
                        MessageType = value;
                        break;

                    case ReplyUriKey:
                        ReplyUri = new Uri(value);
                        break;

                    case ContentTypeKey:
                        ContentType = value;
                        break;

                    case CorrelationIdKey:
                        if (Guid.TryParse(value, out var originalId)) CorrelationId = originalId;
                        break;

                    case SagaIdKey:
                        SagaId = value;
                        break;

                    case CausationIdKey:
                        if (Guid.TryParse(value, out var parentId)) CausationId = parentId;
                        break;

                    case DestinationKey:
                        Destination = new Uri(value);
                        break;

                    case AcceptedContentTypesKey:
                        AcceptedContentTypes = value.Split(',');
                        break;

                    case IdKey:
                        if (Guid.TryParse(value, out var id)) Id = id;
                        break;

                    case ReplyRequestedKey:
                        ReplyRequested = value;
                        break;

                    case AckRequestedKey:
                        AckRequested = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case ExecutionTimeKey:
                        ExecutionTime = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
                        break;

                    case AttemptsKey:
                        Attempts = Int32.Parse(value);
                        break;

                    case DeliverByHeader:
                        DeliverBy = DateTime.Parse(value);
                        break;

                    case ReceivedAtKey:
                        ReceivedAt = new Uri(value);
                        break;


                    default:
                        Headers.Add(key, value);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error trying to read data for {key} = '{value}'", e);
            }
        }
    }
}
