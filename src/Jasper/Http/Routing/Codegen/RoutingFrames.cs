using System;
using System.Collections.Generic;

namespace Jasper.Http.Routing.Codegen
{
    public static class RoutingFrames
    {
        public const string Segments = "segments";

        public static readonly Dictionary<Type, string> TypeOutputs = new Dictionary<Type, string>
        {
            {typeof(bool), "bool"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(char), "char"},
            {typeof(decimal), "decimal"},
            {typeof(float), "float"},
            {typeof(short), "short"},
            {typeof(int), "int"},
            {typeof(long), "long"},
            {typeof(ushort), "ushort"},
            {typeof(uint), "uint"},
            {typeof(ulong), "ulong"},
            {typeof(Guid), typeof(Guid).FullName},
            {typeof(DateTime), typeof(DateTime).FullName},
            {typeof(DateTimeOffset), typeof(DateTimeOffset).FullName}
        };

        public static bool CanBeRouteArgument(Type type)
        {
            if (type == null) return false;
            return type == typeof(string) || TypeOutputs.ContainsKey(type);
        }

        public static bool CanParse(Type argType)
        {
            return TypeOutputs.ContainsKey(argType);
        }
    }
}
