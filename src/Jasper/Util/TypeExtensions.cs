using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Baseline;
using Jasper.Conneg;

namespace Jasper.Util
{
    /// <summary>
    ///     Used to override Jasper's default behavior for identifying a message type.
    ///     Useful for integrating with other services without having to share a DTO
    ///     type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageIdentityAttribute : Attribute
    {
        public MessageIdentityAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }

        public int Version { get; set; }

        public string GetName()
        {
            return Version == 0 ? Alias : $"{Alias}.V{Version}";
        }
    }

    public static class TypeExtensions
    {
        private static readonly Regex _aliasSanitizer = new Regex("<|>", RegexOptions.Compiled);

        private static ImHashMap<Type, string> _typeNames = ImHashMap<Type, string>.Empty;


        private static readonly Type[] _tupleTypes =
        {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>)
        };

        public static bool IsInputTypeCandidate(this Type type)
        {
            if (!type.IsConcrete()) return false;
            if (type.IsSimple()) return false;
            if (type.IsDateTime()) return false;
            if (type == typeof(DateTimeOffset)) return false;
            if (type == typeof(Guid)) return false;

            if (type.Name.EndsWith("Settings")) return false;

            if (type.GetTypeInfo().Assembly == typeof(TypeExtensions).GetTypeInfo().Assembly) return false;

            return true;
        }

        public static string GetPrettyName(this Type t)
        {
            if (!t.GetTypeInfo().IsGenericType)
                return t.Name;

            var sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.Ordinal)));
            sb.Append(t.GetTypeInfo().GetGenericArguments().Aggregate("<",
                (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetPrettyName(type)));
            sb.Append(">");

            return sb.ToString();
        }

        public static string ToMessageTypeName(this Type type)
        {
            if (_typeNames.TryFind(type, out var alias)) return alias;

            var name = toMessageTypeName(type);
            _typeNames = _typeNames.AddOrUpdate(type, name);

            return name;
        }

        private static string toMessageTypeName(Type type)
        {
            if (type.HasAttribute<MessageIdentityAttribute>())
                return type.GetAttribute<MessageIdentityAttribute>().GetName();

            if (type.CanBeCastTo<ClientMessage>() && type.IsConcreteWithDefaultCtor())
                return Activator.CreateInstance(type).As<ClientMessage>().Type;

            if (type.Closes(typeof(IForwardsTo<>)))
            {
                var forwardedType = type.FindInterfaceThatCloses(typeof(IForwardsTo<>)).GetGenericArguments().Single();
                return forwardedType.ToMessageTypeName();
            }

            var nameToAlias = type.FullName;
            if (type.GetTypeInfo().IsGenericType)
                nameToAlias = _aliasSanitizer.Replace(type.GetPrettyName(), string.Empty);

            var parts = new List<string> {nameToAlias};
            if (type.IsNested) parts.Insert(0, type.DeclaringType.Name);

            return string.Join("_", parts);
        }

        public static bool IsValueTuple(this Type type)
        {
            return type != null && type.IsGenericType && _tupleTypes.Contains(type.GetGenericTypeDefinition());
        }
    }
}
