using System;
using System.Reflection;
using Baseline;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Routing
{
    public class RoutingRule
    {
        public RoutingRule()
        {
        }

        public RoutingRule(Assembly assembly)
        {
            Scope = RoutingScope.Assembly;
            Value = assembly.GetName().Name;
        }


        public bool Matches(Type type)
        {
            switch (Scope)
            {
                case RoutingScope.Assembly:
                    return type.Assembly.GetName().Name.EqualsIgnoreCase(Value);

                case RoutingScope.Namespace:
                    return type.IsInNamespace(Value);

                case RoutingScope.Type:
                    return type.Name.EqualsIgnoreCase(Value) || type.FullName.EqualsIgnoreCase(Value);

                case RoutingScope.TypeName:
                    return type.ToMessageTypeName().EqualsIgnoreCase(Value);

                default:
                    return true;
            }
        }

        public RoutingScope Scope { get; set; } = RoutingScope.TypeName;
        public string Value { get; set; }

        // TODO -- add in ContentType
        public static RoutingRule ForType<T>()
        {
            return ForType(typeof(T));
        }

        public static RoutingRule ForType(Type type)
        {
            return new RoutingRule
            {
                Scope = RoutingScope.Type,
                Value = type.FullName
            };
        }

        public static RoutingRule All()
        {
            return new RoutingRule
            {
                Scope = RoutingScope.All
            };
        }
    }
}
