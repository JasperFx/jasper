using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Http.Routing.Codegen;
using Jasper.Util;
using TypeExtensions = Jasper.Internals.Util.TypeExtensions;

namespace Jasper.Http.Routing
{
    // This is mostly tested through Storyteller specs
    public static class RouteBuilder
    {
        public const string Index = "Index";
        public static readonly string[] SpecialClassNames = new string[]{"HomeEndpoint", "ServiceEndpoint"};
        public static readonly IList<string> InputTypeNames = new List<string> {"input", "query", "message", "body"};
        public static readonly Dictionary<string, string> SpecialMethodNames;

        static RouteBuilder()
        {
            SpecialMethodNames = new Dictionary<string, string>
            {
                {Index, "GET"},
            };

            foreach (var httpVerb in HttpVerbs.All)
            {
                SpecialMethodNames.Add(httpVerb.ToLower().Capitalize(), httpVerb.ToUpper());
            }
        }

        public static Route Build<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            return Build(typeof (T), method);
        }

        public static Route Build(Type handlerType, MethodInfo method)
        {
            return Build(method.Name, handlerType, method);
        }

        public static Route Build(string pattern, Type handlerType, MethodInfo method)
        {
            pattern = pattern.Replace("___", "-").Replace("__", "_@");

            var parts = pattern.Split('_')
                .Select(x => x.Replace("@", "_")).ToArray();


            var verb = HttpVerbs.All.FirstOrDefault(x => x.EqualsIgnoreCase(parts[0]));
            if (verb.IsNotEmpty())
            {
                parts = parts.Skip(1).ToArray();
            }
            else
            {
                verb = HttpVerbs.GET;
            }

            var segments = parts
                .Select((x, position) => new Segment(x.ToLowerInvariant(), position))
                .OfType<ISegment>()
                .ToArray();

            if (SpecialClassNames.Contains(handlerType.Name) && SpecialMethodNames.ContainsKey(method.Name))
            {
                pattern = "/";
                verb = SpecialMethodNames[method.Name];
                segments = new ISegment[0];
            }


            // TODO -- eliminate the old Fubu input model routing?
            Type inputType = DetermineInputType(method);

            var hasPrimitives = method.GetParameters().Any(x => x.ParameterType == typeof(string) || RoutingFrames.CanParse(x.ParameterType));
            if (hasPrimitives)
            {
                for (var i = 0; i < segments.Length; i++)
                {
                    var current = parts[i];
                    var parameter = method.GetParameters().FirstOrDefault(x => x.Name == current);
                    if (parameter != null)
                    {
                        var argument = new RouteArgument(parameter, i);
                        segments[i] = argument;
                    }
                }
            }
            else if (inputType != null)
            {
                var members = inputType.GetProperties().OfType<MemberInfo>().Concat(inputType.GetFields()).ToArray();

                for (var i = 0; i < segments.Length; i++)
                {
                    var current = parts[i];
                    var member = members.FirstOrDefault(x => x.Name == current);
                    if (member != null)
                    {
                        var argument = new RouteArgument(member, i);
                        segments[i] = argument;
                    }
                }

            }

            if (method.GetParameters().Any(x => x.IsSpread()))
            {
                segments = segments.Concat(new ISegment[] {new Spread(segments.Length)}).ToArray();
            }

            var route = new Route(segments, verb)
            {
                HandlerType = handlerType,
                Method = method,
                InputType = inputType
            };

            method.ForAttribute<RouteNameAttribute>(att => route.Name = att.Name);

            return route;
        }

        public static Type DetermineInputType(MethodInfo method)
        {
            var first = method.GetParameters().FirstOrDefault();
            if (first == null) return null;

            if (InputTypeNames.Contains(first.Name, StringComparer.OrdinalIgnoreCase))
            {
                return first.ParameterType;
            }

            return first.ParameterType.IsInputTypeCandidate() ? first.ParameterType : null;
        }


    }


}
