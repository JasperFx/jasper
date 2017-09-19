using System;
using System.Collections.Generic;
using System.Xml;
using Baseline;
using Jasper.Util;

namespace Jasper.Bus.Runtime
{
    public partial class Envelope
    {


        public void ReadData(string key, string value)
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
                        ReplyUri = value.ToUri();
                        break;

                    case ContentTypeKey:
                        ContentType = value;
                        break;

                    case OriginalIdKey:
                        OriginalId = value;
                        break;

                    case ParentIdKey:
                        ParentId = value;
                        break;

                    case ResponseIdKey:
                        ResponseId = value;
                        break;

                    case DestinationKey:
                        Destination = value.ToUri();
                        break;

                    case AcceptedContentTypesKey:
                        AcceptedContentTypes = value.Split(',');
                        break;

                    case IdKey:
                        CorrelationId = value;
                        break;

                    case ReplyRequestedKey:
                        ReplyRequested = value;
                        break;

                    case AckRequestedKey:
                        AckRequested = value.EqualsIgnoreCase("true");
                        break;

                    case ExecutionTimeKey:
                        ExecutionTime = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
                        break;

                    case AttemptsKey:
                        Attempts = int.Parse(value);
                        break;

                    case DeliverByHeader:
                        DeliverBy = DateTime.Parse(value);
                        break;


                    case SentAttemptsHeaderKey:
                        SentAttempts = int.Parse(value);
                        break;

                    case ReceivedAtKey:
                        ReceivedAt = value.ToUri();
                        break;

                    default:
                        Headers.Add(key, value);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error trying to read data for {key} = '{value}'");
            }
        }
    }
}
