using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jasper.Configuration
{
    public class Subscription
    {
        private string[] _contentTypes = {"application/json"};

        public Subscription()
        {
        }

        public Subscription(Assembly assembly)
        {
            Scope = RoutingScope.Assembly;
            Match = assembly.GetName().Name;
        }

        /// <summary>
        /// How does this rule apply? For all messages? By Namespace? By Assembly?
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RoutingScope Scope { get; set; } = RoutingScope.All;

        /// <summary>
        /// The outgoing address to send matching messages
        /// </summary>
        [Obsolete("Get rid of this")]
        public Uri Uri { get; set; }

        /// <summary>
        /// The legal, accepted content types for the receivers. The default is ["application/json"]
        /// </summary>
        public string[] ContentTypes
        {
            get => _contentTypes;
            set => _contentTypes = value?.Distinct().ToArray() ?? new[] {"application/json"};
        }

        /// <summary>
        /// A type name or namespace name if matching on type or namespace
        /// </summary>
        public string Match { get; set; } = string.Empty;

        /// <summary>
        /// Create a subscription for a specific message type
        /// </summary>
        /// <param name="uri"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Subscription ForType<T>(Uri uri)
        {
            return ForType(typeof(T), uri);
        }

        /// <summary>
        /// Create a subscription for a specific message type
        /// </summary>
        /// <param name="uriString"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Subscription ForType<T>(string uriString)
        {
            return ForType(typeof(T), uriString.ToUri());
        }


        /// <summary>
        /// Create a subscription for a specific message type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Subscription ForType(Type type, Uri uri)
        {
            return new Subscription
            {
                Scope = RoutingScope.Type,
                Match = type.FullName,
                Uri = uri
            };
        }

        /// <summary>
        /// Create a subscription for all messages published in this application
        /// </summary>
        /// <returns></returns>
        public static Subscription All()
        {
            return new Subscription
            {
                Scope = RoutingScope.All
            };
        }

        public bool Matches(Type type)
        {
            switch (Scope)
            {
                case RoutingScope.Assembly:
                    return type.Assembly.GetName().Name.EqualsIgnoreCase(Match);

                case RoutingScope.Namespace:
                    return type.IsInNamespace(Match);

                case RoutingScope.Type:
                    return type.Name.EqualsIgnoreCase(Match) || type.FullName.EqualsIgnoreCase(Match) ||
                           type.ToMessageTypeName().EqualsIgnoreCase(Match);

                case RoutingScope.TypeName:
                    return type.ToMessageTypeName().EqualsIgnoreCase(Match);

                default:
                    return true;
            }
        }


        protected bool Equals(Subscription other)
        {
            return Scope == other.Scope && Equals(Uri, other.Uri) && ContentTypes.SequenceEqual(other.ContentTypes) &&
                   string.Equals(Match, other.Match);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Subscription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Scope;
                hashCode = (hashCode * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContentTypes != null ? ContentTypes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Match != null ? Match.GetHashCode() : 0);
                return hashCode;
            }
        }


    }
}
