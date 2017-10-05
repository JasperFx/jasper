using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Jasper.Remotes.Messaging
{
    // TODO -- move this to the WebSockets
    public static class JsonSerialization
    {
        private static readonly LightweightCache<string, Type> _messageTypes = new LightweightCache<string, Type>();

        public static void RegisterTypesFrom(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(x => x.IsConcreteWithDefaultCtor() && x.IsConcreteTypeOf<ClientMessage>());

            types.Each(x =>
            {
                var message = Activator.CreateInstance(x).As<ClientMessage>();
                _messageTypes[message.Type] = x;
            });
        }

        public static void RegisterType(string name, Type type)
        {
            _messageTypes[name] = type;
        }

        public static Type TypeForJson(string json)
        {
            var token = JToken.Parse(json);
            var type = token.Value<string>("type");

            return (!type.IsEmpty() && _messageTypes.Has(type))
                ?_messageTypes[type]
                : null;
        }

        public static string ToJson(object o, bool indentedFormatting = false)
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,

            };

            serializer.Converters.Add(new StringEnumConverter());

            if (indentedFormatting)
            {
                serializer.Formatting = Formatting.Indented;
            }

            var writer = new StringWriter();
            serializer.Serialize(writer, o);

            return writer.ToString();
        }

        public static string ToCleanJson(this object o, bool indentedFormatting = false, IContractResolver contractResolver = null)
        {
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.None,  };
            serializer.Converters.Add(new StringEnumConverter());

            serializer.ContractResolver = contractResolver ?? new CamelCasePropertyNamesContractResolver();

            if (indentedFormatting)
            {
                serializer.Formatting = Formatting.Indented;
            }

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    serializer.Serialize(writer, o);

                    writer.Flush();

                    stream.Position = 0;

                    return stream.ReadAllText();
                }
            }
        }

        public static Task WriteCleanJson(Stream response, object o)
        {
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.None, };
            serializer.Converters.Add(new StringEnumConverter());

            var raw = new MemoryStream();

            using (var writer = new StreamWriter(raw, Encoding.UTF8))
            {
                serializer.Serialize(writer, o);

                writer.Flush();

                raw.Position = 3;

                return raw.CopyToAsync(response);
            }
        }

        public static string ToIndentedJson(object o)
        {
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.None, Formatting = Formatting.Indented};
            serializer.Converters.Add(new StringEnumConverter());

            var writer = new StringWriter();
            serializer.Serialize(writer, o);

            return writer.ToString();
        }


        public static string FormatJson(this string json)
        {
            return JToken.Parse(json).ToString(Formatting.Indented);
        }

        public static T Deserialize<T>(string json)
        {
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.All };
            serializer.Converters.Add(new StringEnumConverter());

            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        public static object DeserializeMessage(string json)
        {
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.All };
            serializer.Converters.Add(new StringEnumConverter());

            var jsonTextReader = new JsonTextReader(new StringReader(json));

            var messageType = TypeForJson(json);

            return messageType != null
                ? serializer.Deserialize(jsonTextReader, messageType)
                : serializer.Deserialize(jsonTextReader);
        }
    }
}
