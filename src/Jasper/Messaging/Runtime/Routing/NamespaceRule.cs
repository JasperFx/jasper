using System;
using Baseline;

namespace Jasper.Messaging.Runtime.Routing
{
    public class NamespaceRule : IRoutingRule
    {
        public NamespaceRule(string @namespace)
        {
            Namespace = @namespace;
        }

        public string Namespace { get; }

        public bool Matches(Type type)
        {
            return type.IsInNamespace(Namespace);
        }

        public override string ToString()
        {
            return "Messages from namespace " + Namespace;
        }

        public static NamespaceRule For<T>()
        {
            return new NamespaceRule(typeof(T).Namespace);
        }

        protected bool Equals(NamespaceRule other)
        {
            return string.Equals(Namespace, other.Namespace);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamespaceRule) obj);
        }

        public override int GetHashCode()
        {
            return (Namespace != null ? Namespace.GetHashCode() : 0);
        }
    }
}