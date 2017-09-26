using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlueMilk.Util
{
    internal static class TypeExtensions
    {
        private static readonly IList<Type> _integerTypes = new List<Type>
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(byte?),
            typeof(short?),
            typeof(int?),
            typeof(long?),
            typeof(sbyte?),
            typeof(ushort?),
            typeof(uint?),
            typeof(ulong?)
        };

        internal static bool HasConstructors(this Type type)
        {
            return type.GetTypeInfo().GetConstructors().Any();
        }

        internal static IEnumerable<Type> AllInterfaces(this Type type)
        {
            foreach (var @interface in type.GetTypeInfo().GetInterfaces())
            {
                yield return @interface;
            }
        }

        /// <summary>
        ///     Does a hard cast of the object to T.  *Will* throw InvalidCastException
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static T As<T>(this object target)
        {
            return (T) target;
        }

        internal static bool IsNullableOfT(this Type theType)
        {
            if (theType == null) return false;

            return theType.GetTypeInfo().IsGenericType && theType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool IsNullableOf(this Type theType, Type otherType)
        {
            return theType.IsNullableOfT() && theType.GetTypeInfo().GetGenericArguments()[0] == otherType;
        }

        internal static bool IsTypeOrNullableOf<T>(this Type theType)
        {
            var otherType = typeof(T);
            return theType == otherType ||
                   (theType.IsNullableOfT() && theType.GetTypeInfo().GetGenericArguments()[0] == otherType);
        }

        internal static bool CanBeCastTo<T>(this Type type)
        {
            if (type == null) return false;
            var destinationType = typeof(T);

            return CanBeCastTo(type, destinationType);
        }

        internal static bool CanBeCastTo(this Type type, Type destinationType)
        {
            if (type == null) return false;
            if (type == destinationType) return true;

            return destinationType.GetTypeInfo().IsAssignableFrom(type);
        }

        internal static bool IsInNamespace(this Type type, string nameSpace)
        {
            if (type == null) return false;

            return type.Namespace.StartsWith(nameSpace);
        }

        internal static bool IsOpenGeneric(this Type type)
        {
            if (type == null) return false;
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericTypeDefinition || typeInfo.ContainsGenericParameters;
        }

        internal static bool IsGenericEnumerable(this Type type)
        {
            if (type == null) return false;

            var genericArgs = type.GetTypeInfo().GetGenericArguments();
            return genericArgs.Length == 1 && typeof(IEnumerable<>).MakeGenericType(genericArgs).GetTypeInfo().IsAssignableFrom(type);
        }

        internal static bool IsConcreteTypeOf<T>(this Type pluggedType)
        {
            if (pluggedType == null) return false;

            return pluggedType.IsConcrete() && typeof(T).GetTypeInfo().IsAssignableFrom(pluggedType);
        }

        internal static bool ImplementsInterfaceTemplate(this Type pluggedType, Type templateType)
        {
            if (!pluggedType.IsConcrete()) return false;

            foreach (var interfaceType in pluggedType.GetTypeInfo().GetInterfaces())
            {
                if (interfaceType.GetTypeInfo().IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == templateType)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsConcreteWithDefaultCtor(this Type type)
        {
            return type.IsConcrete() && type.GetTypeInfo().GetConstructor(new Type[0]) != null;
        }

        internal static Type FindInterfaceThatCloses(this Type type, Type openType)
        {
            if (type == typeof(object)) return null;

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsInterface && typeInfo.IsGenericType && type.GetGenericTypeDefinition() == openType)
                return type;


            foreach (var interfaceType in type.GetTypeInfo().GetInterfaces())
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
                if (interfaceTypeInfo.IsGenericType && interfaceType.GetGenericTypeDefinition() == openType)
                {
                    return interfaceType;
                }
            }

            if (!type.IsConcrete()) return null;


            return typeInfo.BaseType == typeof(object)
                ? null
                : typeInfo.BaseType.FindInterfaceThatCloses(openType);
        }

        internal static Type FindParameterTypeTo(this Type type, Type openType)
        {
            var interfaceType = type.FindInterfaceThatCloses(openType);
            return interfaceType?.GetTypeInfo().GetGenericArguments().FirstOrDefault();
        }

        internal static bool IsNullable(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool Closes(this Type type, Type openType)
        {
            if (type == null) return false;

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == openType) return true;

            foreach (var @interface in type.GetTypeInfo().GetInterfaces())
            {
                if (@interface.Closes(openType)) return true;
            }

            var baseType = typeInfo.BaseType;
            if (baseType == null) return false;

            var baseTypeInfo = baseType.GetTypeInfo();

            var closes = baseTypeInfo.IsGenericType && baseType.GetGenericTypeDefinition() == openType;
            if (closes) return true;

            return typeInfo.BaseType?.Closes(openType) ?? false;
        }

        internal static Type GetInnerTypeFromNullable(this Type nullableType)
        {
            return nullableType.GetTypeInfo().GetGenericArguments()[0];
        }


        internal static string GetName(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                string[] parameters = type.GetTypeInfo().GetGenericArguments().Select(x => x.GetName()).ToArray();
                var parameterList = string.Join(", ", parameters);
                return "{0}<{1}>".ToFormat(type.Name, parameterList);
            }

            return type.Name;
        }

        internal static string GetFullName(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                string[] parameters = type.GetTypeInfo().GetGenericArguments().Select(x => x.GetName()).ToArray();
                var parameterList = string.Join(", ", parameters);
                return "{0}<{1}>".ToFormat(type.Name, parameterList);
            }

            return type.FullName;
        }


        internal static bool IsString(this Type type)
        {
            return type == typeof(string);
        }

        internal static bool IsPrimitive(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive && !IsString(type) && type != typeof(IntPtr);
        }

        internal static bool IsSimple(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || IsString(type) || typeInfo.IsEnum;
        }

        internal static bool IsConcrete(this Type type)
        {
            if (type == null) return false;

            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsAbstract && !typeInfo.IsInterface;
        }

        internal static bool IsNotConcrete(this Type type)
        {
            return !type.IsConcrete();
        }

        /// <summary>
        ///     Returns true if the type is a DateTime or nullable DateTime
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <returns></returns>
        internal static bool IsDateTime(this Type typeToCheck)
        {
            return typeToCheck == typeof(DateTime) || typeToCheck == typeof(DateTime?);
        }

        internal static bool IsBoolean(this Type typeToCheck)
        {
            return typeToCheck == typeof(bool) || typeToCheck == typeof(bool?);
        }

        /// <summary>
        ///     Displays type names using CSharp syntax style. Supports funky generic types.
        /// </summary>
        /// <param name="type">Type to be pretty printed</param>
        /// <returns></returns>
        internal static string PrettyPrint(this Type type)
        {
            return type.PrettyPrint(t => t.Name);
        }

        /// <summary>
        ///     Displays type names using CSharp syntax style. Supports funky generic types.
        /// </summary>
        /// <param name="type">Type to be pretty printed</param>
        /// <param name="selector">
        ///     Function determining the name of the type to be displayed. Useful if you want a fully qualified
        ///     name.
        /// </param>
        /// <returns></returns>
        internal static string PrettyPrint(this Type type, Func<Type, string> selector)
        {
            var typeName = selector(type) ?? string.Empty;
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return typeName;
            }

            var genericParamSelector = typeInfo.IsGenericTypeDefinition ? t => t.Name : selector;
            var genericTypeList = string.Join(",", type.GetTypeInfo().GetGenericArguments().Select(genericParamSelector).ToArray());
            var tickLocation = typeName.IndexOf('`');
            if (tickLocation >= 0)
            {
                typeName = typeName.Substring(0, tickLocation);
            }
            return $"{typeName}<{genericTypeList}>";
        }

        /// <summary>
        ///     Returns a boolean value indicating whether or not the type is:
        ///     int, long, decimal, short, float, or double
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Bool indicating whether the type is numeric</returns>
        internal static bool IsNumeric(this Type type)
        {
            return type.IsFloatingPoint() || type.IsIntegerBased();
        }


        /// <summary>
        ///     Returns a boolean value indicating whether or not the type is:
        ///     int, long or short
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Bool indicating whether the type is integer based</returns>
        internal static bool IsIntegerBased(this Type type)
        {
            return _integerTypes.Contains(type);
        }

        /// <summary>
        ///     Returns a boolean value indicating whether or not the type is:
        ///     decimal, float or double
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Bool indicating whether the type is floating point</returns>
        internal static bool IsFloatingPoint(this Type type)
        {
            return type == typeof(decimal) || type == typeof(float) || type == typeof(double);
        }


        internal static T CloseAndBuildAs<T>(this Type openType, params Type[] parameterTypes)
        {
            var closedType = openType.MakeGenericType(parameterTypes);
            return (T) Activator.CreateInstance(closedType);
        }

        internal static T CloseAndBuildAs<T>(this Type openType, object ctorArgument, params Type[] parameterTypes)
        {
            var closedType = openType.MakeGenericType(parameterTypes);
            return (T) Activator.CreateInstance(closedType, ctorArgument);
        }

        internal static T CloseAndBuildAs<T>(this Type openType, object ctorArgument1, object ctorArgument2,
            params Type[] parameterTypes)
        {
            var closedType = openType.MakeGenericType(parameterTypes);
            return (T) Activator.CreateInstance(closedType, ctorArgument1, ctorArgument2);
        }

        internal static bool PropertyMatches(this PropertyInfo prop1, PropertyInfo prop2)
        {
            return prop1.DeclaringType == prop2.DeclaringType && prop1.Name == prop2.Name;
        }

        internal static T Create<T>(this Type type)
        {
            return (T) type.Create();
        }

        internal static object Create(this Type type)
        {
            return Activator.CreateInstance(type);
        }


        internal static Type DeriveElementType(this Type type)
        {
            return type.GetElementType() ?? type.GetTypeInfo().GetGenericArguments().FirstOrDefault();
        }

        internal static Type IsAnEnumerationOf(this Type type)
        {
            if (!type.Closes(typeof(IEnumerable<>)))
            {
                throw new Exception("Duh, its gotta be enumerable");
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.GetTypeInfo().IsGenericType)
            {
                return type.GetTypeInfo().GetGenericArguments()[0];
            }


            throw new Exception($"I don't know how to figure out what this is a collection of. Can you tell me? {type}");
        }


        internal static void ForAttribute<T>(this Type type, Action<T> action) where T : Attribute
        {
            var atts = type.GetTypeInfo().GetCustomAttributes(typeof(T));
            foreach (T att in atts)
            {
                action(att);
            }
        }

        internal static void ForAttribute<T>(this Type type, Action<T> action, Action elseDo)
            where T : Attribute
        {
            var atts = type.GetTypeInfo().GetCustomAttributes(typeof(T)).ToArray();
            foreach (T att in atts)
            {
                action(att);
            }

            if (!atts.Any())
            {
                elseDo();
            }
        }

        internal static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>().Any();
        }

        internal static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>().FirstOrDefault();
        }

        public static Type FindFirstInterfaceThatCloses(this Type TPluggedType, Type templateType)
        {
            return TPluggedType.FindInterfacesThatClose(templateType).FirstOrDefault();
        }

        public static IEnumerable<Type> FindInterfacesThatClose(this Type TPluggedType, Type templateType)
        {
            return rawFindInterfacesThatCloses(TPluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> rawFindInterfacesThatCloses(Type TPluggedType, Type templateType)
        {
            if (!TPluggedType.IsConcrete()) yield break;

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                    TPluggedType.GetTypeInfo().GetInterfaces()
                        .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (TPluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     (TPluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType))
            {
                yield return TPluggedType.GetTypeInfo().BaseType;
            }

            if (TPluggedType.GetTypeInfo().BaseType == typeof(object)) yield break;

            foreach (var interfaceType in rawFindInterfacesThatCloses(TPluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }
    }
}
