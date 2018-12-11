using System;
using System.Collections.Generic;
using System.Xml;
using Jasper.Util;

namespace Jasper.Messaging.Runtime
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
                    dict.Remove(key);
                else
                    dict[key] = value.ToString();
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

        public static DateTime? GetNullableDateTime(this IDictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key)) return XmlConvert.ToDateTime(dict[key], XmlDateTimeSerializationMode.Utc);

            return null;
        }
    }
}
