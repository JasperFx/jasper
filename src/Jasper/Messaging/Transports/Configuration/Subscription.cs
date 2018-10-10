using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jasper.Messaging.Transports.Configuration
{
    public class Subscription
    {
        private string[] _contentTypes = new string[]{"application/json"};

        public Subscription()
        {
        }

        public Subscription(Assembly assembly)
        {
            Scope = RoutingScope.Assembly;
            Match = assembly.GetName().Name;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public RoutingScope Scope { get; set; } = RoutingScope.All;
        public Uri Uri { get; set; }

        public string[] ContentTypes
        {
            get => _contentTypes;
            set => _contentTypes = value?.Distinct().ToArray() ?? new string[]{"application/json"};
        }

        public string Match { get; set; } = string.Empty;

        public static Subscription ForType<T>(Uri uri = null)
        {
            return ForType(typeof(T), uri);
        }

        public static Subscription ForType(Type type, Uri uri = null)
        {
            return new Subscription
            {
                Scope = RoutingScope.Type,
                Match = type.FullName,
                Uri = uri
            };
        }

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
                    return type.Name.EqualsIgnoreCase(Match) || type.FullName.EqualsIgnoreCase(Match) || type.ToMessageTypeName().EqualsIgnoreCase(Match);

                case RoutingScope.TypeName:
                    return type.ToMessageTypeName().EqualsIgnoreCase(Match);

                default:
                    return true;
            }
        }


        protected bool Equals(Subscription other)
        {
            return Scope == other.Scope && Equals(Uri, other.Uri) && ContentTypes.SequenceEqual(other.ContentTypes) && string.Equals(Match, other.Match);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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

        public static Subscription All(Uri uri)
        {
            var subscription = Subscription.All();
            subscription.Uri = uri;

            return subscription;
        }
    }
}
