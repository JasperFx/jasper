using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Baseline;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> Clone(this IDictionary<string, string> dict)
        {
            return new Dictionary<string, string>(dict);
        }

        public static string Get(this IDictionary<string, string> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        public static void Set(this IDictionary<string, string> dict, string key, object value)
        {
            if (dict.ContainsKey(key))
            {
                if (value == null)
                {
                    dict.Remove(key);
                }
                else
                {
                    dict[key] = value.ToString();
                }
            }
            else
            {
                dict.Add(key, value?.ToString());
            }
        }

        public static Uri GetUri(this IDictionary<string, string> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key].ToUri() : null;
        }

        public static int GetInt(this IDictionary<string, string> dict, string key)
        {
            return dict.ContainsKey(key) ? int.Parse(dict[key]) : 0;
        }
    }

    public class HeaderWrapper : IContentTypeAware
    {
        public Uri Source
        {
            get { return Headers.GetUri(Envelope.SourceKey); }
            set { Headers.Set(Envelope.SourceKey, value); }
        }

        public string MessageType
        {
            get { return Headers.Get(Envelope.MessageTypeKey); }
            set
            {
                Headers.Set(Envelope.MessageTypeKey, value);
            }
        }

        public Uri ReplyUri
        {
            get { return Headers.GetUri(Envelope.ReplyUriKey); }
            set { Headers.Set(Envelope.ReplyUriKey, value); }
        }

        public string ContentType
        {
            get { return Headers.Get(Envelope.ContentTypeKey); }
            set { Headers.Set(Envelope.ContentTypeKey, value); }
        }

        public string OriginalId
        {
            get { return Headers.Get(Envelope.OriginalIdKey); }
            set { Headers.Set(Envelope.OriginalIdKey, value); }
        }

        public string ParentId
        {
            get { return Headers.Get(Envelope.ParentIdKey); }
            set { Headers.Set(Envelope.ParentIdKey, value); }
        }

        public string ResponseId
        {
            get { return Headers.Get(Envelope.ResponseIdKey); }
            set { Headers.Set(Envelope.ResponseIdKey, value); }
        }

        public Uri Destination
        {
            get { return Headers.GetUri(Envelope.DestinationKey); }
            set { Headers.Set(Envelope.DestinationKey, value); }
        }

        public Uri ReceivedAt
        {
            get { return Headers.GetUri(Envelope.ReceivedAtKey); }
            set { Headers.Set(Envelope.ReceivedAtKey, value); }

        }

        public string[] AcceptedContentTypes
        {
            get
            {
                var raw = Headers.Get(Envelope.AcceptedContentTypesKey);
                if (raw.IsEmpty()) return new string[0];

                return raw.Split(',');
            }
            set { Headers.Set(Envelope.AcceptedContentTypesKey, value?.Join(",")); }
        }

        public IDictionary<string, string> Headers { get; set; }

        public string CorrelationId
        {
            get { return Headers.Get(Envelope.IdKey); }
            set { Headers.Set(Envelope.IdKey, value); }
        }

        public string ReplyRequested
        {
            get { return Headers.Get(Envelope.ReplyRequestedKey); }
            set { Headers.Set(Envelope.ReplyRequestedKey, value); }
        }

        public bool AckRequested
        {
            get
            {
                return Headers.ContainsKey(Envelope.AckRequestedKey) && Headers[Envelope.AckRequestedKey].EqualsIgnoreCase("true");
            }
            set
            {
                if (value)
                {
                    Headers.Set(Envelope.AckRequestedKey, "true");
                }
                else
                {
                    Headers.Remove(Envelope.AckRequestedKey);
                }
            }
        }

        public DateTime? ExecutionTime
        {
            get { return Headers.ContainsKey(Envelope.ExecutionTimeKey) ? XmlConvert.ToDateTime(Headers[Envelope.ExecutionTimeKey], XmlDateTimeSerializationMode.Utc) : (DateTime?)null; }
            set
            {
                if (value == null)
                {
                    Headers.Remove(Envelope.ExecutionTimeKey);
                }
                else
                {
                    var dateString = value.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                    Headers.Set(Envelope.ExecutionTimeKey, dateString);
                }

            }
        }

        public bool IsDelayed(DateTime utcNow)
        {
            if (!Headers.ContainsKey(Envelope.ExecutionTimeKey)) return false;

            return ExecutionTime.Value > utcNow;
        }

        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
    }
}
