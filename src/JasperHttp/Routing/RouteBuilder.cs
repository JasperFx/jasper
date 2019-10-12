using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Util;
using JasperHttp.Routing.Codegen;
using LamarCodeGeneration;

namespace JasperHttp.Routing
{
    // This is mostly tested through Storyteller specs
    public static class RouteBuilder
    {
        public static readonly IList<string> InputTypeNames = new List<string> {"input", "query", "message", "body"};

        public static IList<IPatternRule> PatternRules = new List<IPatternRule>{new HomeEndpointRule(), new RootUrlRoutingRule(), new VerbMethodNames()};

        public static Route Build<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            return Build(typeof(T), method);
        }

        public static Route Build(Type handlerType, MethodInfo method)
        {
            var pattern = PatternRules.FirstValue(x => x.DetermineRoute(handlerType, method));
            if (pattern == null) throw new InvalidOperationException($"Jasper does not know how to make an Http route from the method {handlerType.NameInCode()}.{method.Name}()");

            return Build(pattern, handlerType, method);
        }

        public static Route Build(RoutePattern pattern, Type handlerType, MethodInfo method)
        {
            var inputType = DetermineInputType(method);

            var hasPrimitives = method.GetParameters().Any(x =>
                x.ParameterType == typeof(string) || RoutingFrames.CanParse(x.ParameterType));

            if (hasPrimitives)
            {
                for (var i = 0; i < pattern.Segments.Length; i++)
                {
                    var current = pattern.Segments[i].SegmentPath;
                    var isParameter = current.StartsWith("{") && current.EndsWith("}");
                    var parameterName = current.TrimStart('{').TrimEnd('}');


                    var parameter = method.GetParameters().FirstOrDefault(x => x.Name == parameterName);
                    if (parameter != null)
                    {
                        var argument = new RouteArgument(parameter, i);
                        pattern.Segments[i] = argument;
                    }

                    if (isParameter && parameter == null)
                    {
                        throw new InvalidOperationException($"Required parameter '{current}' could not be resoved in method {handlerType.FullNameInCode()}.{method.Name}()");
                    }
                }
            }
            else if (inputType != null)
            {
                var members = inputType.GetProperties().OfType<MemberInfo>().Concat(inputType.GetFields()).ToArray();

                for (var i = 0; i < pattern.Segments.Length; i++)
                {
                    var current = pattern.Segments[i].SegmentPath;
                    var member = members.FirstOrDefault(x => x.Name == current);
                    if (member != null)
                    {
                        var argument = new RouteArgument(member, i);
                        pattern.Segments[i] = argument;
                    }
                }
            }

            var spreads = method.GetParameters().Where(x => x.IsSpread()).ToArray();
            if (spreads.Length > 1)
                throw new InvalidOperationException(
                    $"An HTTP action method can only take in either '{Route.PathSegments}' or '{Route.RelativePath}', but not both. Error with action {handlerType.FullName}.{method.Name}()");

            var segments = pattern.Segments;
            if (spreads.Length == 1) segments = segments.Concat(new ISegment[] {new Spread(segments.Length)}).ToArray();

            var route = new Route(segments, pattern.Method)
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

            if (first.IsSpread()) return null;

            if (InputTypeNames.Contains(first.Name, StringComparer.OrdinalIgnoreCase)) return first.ParameterType;

            return first.ParameterType.IsInputTypeCandidate() ? first.ParameterType : null;
        }
    }
}
