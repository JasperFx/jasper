using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Baseline;
using Baseline.Conversion;
using Baseline.Reflection;
using Jasper.Http.Routing.Codegen;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;

namespace Jasper.Http.Routing
{
    public interface IRoutingFrameSource
    {
        Frame ToParsingFrame(MethodCall action);
        ParameterInfo Parameter { get; }
    }

    public class RouteArgument : ISegment, IRoutingFrameSource
    {
        public static readonly Conversions Conversions = new Conversions();

        private Type _argType;

        private Func<string, object> _converter = x => x;
        private MemberInfo _mappedMember;

        private Func<object, object> _readData = x => null;
        private Action<object, object> _writeData = (x, y) => { };

        static RouteArgument()
        {
            Conversions.RegisterConversion(Guid.Parse);
            Conversions.RegisterConversion(DateTimeOffset.Parse);
        }

        public RouteArgument(string key, int position, Type argType = null)
        {
            ArgType = argType ?? typeof(string);
            Key = key;
            Position = position;
        }

        public RouteArgument(ParameterInfo parameter, int position)
        {
            MappedParameter = parameter;
            Position = position;
        }

        public ParameterInfo Parameter { get; private set; }


        public string Key { get; private set; }

        public ParameterInfo MappedParameter
        {
            get => Parameter;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                Key = value.Name;
                Parameter = value;
                ArgType = value.ParameterType;
            }
        }

        public MemberInfo MappedMember
        {
            get => _mappedMember;
            set
            {
                if (value is FieldInfo)
                {
                    var field = value.As<FieldInfo>();
                    _writeData = (input, val) => field.SetValue(input, val);
                    _readData = input => field.GetValue(input);
                    ArgType = field.FieldType;

                    Key = field.Name;
                }
                else if (value is PropertyInfo)
                {
                    var prop = value.As<PropertyInfo>();
                    _writeData = (input, val) => prop.SetValue(input, val);
                    _readData = input => prop.GetValue(input);
                    ArgType = prop.PropertyType;

                    Key = prop.Name;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "We don't serve your kind here! Only FieldInfo and PropertyInfo's are supported");
                }

                _mappedMember = value;
            }
        }

        public Type ArgType
        {
            get => _argType;
            set
            {
                _argType = value ?? throw new ArgumentNullException(nameof(value));

                _converter = Conversions.FindConverter(ArgType);

                if (_converter == null)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"Could not find a conversion for type {value.FullName}");
            }
        }

        public Frame ToParsingFrame(MethodCall action)
        {
            if (Parameter == null && action != null)
            {
                Parameter = action.Method.GetParameters().FirstOrDefault(x => x.Name == Key);
                ArgType = Parameter?.ParameterType;
            }

            if (ArgType == null) throw new InvalidOperationException($"Missing an {nameof(ArgType)} value");

            if (ArgType == typeof(string)) return new CastRouteArgumentFrame(typeof(string), Key, Position);

            if (CanParse(ArgType)) return new ParsedRouteArgumentFrame(ArgType, Key, Position);

            throw new InvalidOperationException(
                $"Jasper does not (yet) know how to parse a route argument of type {ArgType.FullNameInCode()}");
        }

        public int Position { get; }

        public string SegmentPath => ":" + Key;

        public string ReadRouteDataFromMethodArguments(List<object> arguments)
        {
            return Parameter == null ? String.Empty : WebUtility.UrlEncode(arguments[Parameter.Position].ToString());
        }

        public string SegmentFromParameters(IDictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(Key)) throw new UrlResolutionException($"Missing required parameter '{Key}'");

            return parameters[Key].ToString();
        }

        public string RoutePatternPath()
        {
            return CanBeRouteConstraint(ArgType) ? $"{{{Key}:{TypeRouteConstraints[ArgType]}}}" : $"{{{Key}}}";
        }

        protected bool Equals(RouteArgument other)
        {
            return String.Equals(Key, other.Key) && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RouteArgument) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ Position;
            }
        }

        public override string ToString()
        {
            return $"{Key}:{Position}";
        }

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
            {typeof(double), "double"},
            {typeof(long), "long"},
            {typeof(ushort), "ushort"},
            {typeof(uint), "uint"},
            {typeof(ulong), "ulong"},
            {typeof(Guid), typeof(Guid).FullName},
            {typeof(DateTime), typeof(DateTime).FullName},
            {typeof(DateTimeOffset), typeof(DateTimeOffset).FullName}
        };

        public static readonly Dictionary<Type, string> TypeRouteConstraints = new Dictionary<Type, string>
        {
            {typeof(bool), "bool"},
            {typeof(string), "alpha"},
            {typeof(decimal), "decimal"},
            {typeof(float), "float"},
            {typeof(int), "int"},
            {typeof(double), "double"},
            {typeof(long), "long"},
            {typeof(Guid), "guid"},
            {typeof(DateTime), "datetime"}
        };

        public static bool CanBeRouteConstraint(Type type)
        {
            return TypeRouteConstraints.ContainsKey(type);
        }

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
