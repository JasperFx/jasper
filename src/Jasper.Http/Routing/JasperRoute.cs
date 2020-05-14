using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Http.Routing.Codegen;
using Jasper.Util;
using LamarCodeGeneration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using RouteHandler = Jasper.Http.Model.RouteHandler;

namespace Jasper.Http.Routing
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSpread(this ParameterInfo parameter)
        {
            if (parameter.Name == JasperRoute.RelativePath && parameter.ParameterType == typeof(string)) return true;
            if (parameter.Name == JasperRoute.PathSegments && parameter.ParameterType == typeof(string[])) return true;
            return false;
        }
    }

    public class JasperRoute
    {
        public static readonly IList<string> InputTypeNames = new List<string> {"input", "query", "message", "body"};

        public static IList<IRoutingRule> Rules = new List<IRoutingRule>
            {new HomeEndpointRule(), new RootUrlRoutingRule(), new VerbMethodNames()};

        public static JasperRoute Build<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            return Build(typeof(T), method);
        }

        public static JasperRoute Build(Type handlerType, MethodInfo method)
        {
            var route = Rules.FirstValue(x => x.DetermineRoute(handlerType, method));
            if (route == null)
                throw new InvalidOperationException(
                    $"Jasper does not know how to make an Http route from the method {handlerType.NameInCode()}.{method.Name}()");

            route.InputType = DetermineInputType(method);
            route.HandlerType = handlerType;
            route.Method = method;

            var hasPrimitives = method.GetParameters().Any(x =>
                x.ParameterType == typeof(string) || RouteArgument.CanParse(x.ParameterType));

            if (hasPrimitives)
            {
                for (var i = 0; i < route.Segments.Count; i++)
                {
                    var current = route.Segments[i].SegmentPath;
                    var isParameter = current.StartsWith("{") && current.EndsWith("}");
                    var parameterName = current.TrimStart('{').TrimEnd('}');


                    var parameter = method.GetParameters().FirstOrDefault(x => x.Name == parameterName);
                    if (parameter != null)
                    {
                        var argument = new RouteArgument(parameter, i);
                        route.Segments[i] = argument;
                    }

                    if (isParameter && parameter == null)
                        throw new InvalidOperationException(
                            $"Required parameter '{current}' could not be resoved in method {handlerType.FullNameInCode()}.{method.Name}()");
                }
            }

            var spreads = method.GetParameters().Where(x => x.IsSpread()).ToArray();
            if (spreads.Length > 1)
                throw new InvalidOperationException(
                    $"An HTTP action method can only take in either '{JasperRoute.PathSegments}' or '{JasperRoute.RelativePath}', but not both. Error with action {handlerType.FullName}.{method.Name}()");

            if (spreads.Length == 1)
            {
                var spread = new Spread(route.Segments.Count, spreads.Single());
                route.Segments.Add(spread);
            }

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


        public const string RelativePath = "relativePath";
        public const string PathSegments = "pathSegments";

        private Lazy<RouteArgument[]> _arguments;
        private Spread _spread;


        public JasperRoute(string httpMethod, string pattern)
        {
            pattern = pattern?.TrimStart('/').TrimEnd('/') ?? throw new ArgumentNullException(nameof(pattern));


            HttpMethod = httpMethod;

            if (pattern.IsNotEmpty())
            {
                var segments = pattern.Split('/');
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = ToParameter(segments[i], i);
                    Segments.Add(segment);
                }

                validateSegments();
            }

            Name = $"{HttpMethod}:/{Pattern}";

            setupArgumentsAndSpread();
        }

        public JasperRoute(ISegment[] segments, string httpVerb)
        {
            Segments.AddRange(segments);

            validateSegments();

            HttpMethod = httpVerb;


            Name = $"{HttpMethod}:{Pattern}";

            setupArgumentsAndSpread();
        }

        public string Description => $"{HttpMethod}: {Pattern}";

        public List<ISegment> Segments { get; } = new List<ISegment>();

        public Type InputType { get; set; }
        public Type HandlerType { get; set; }
        public MethodInfo Method { get; set; }

        public bool HasParameters => HasSpread || _arguments.Value.Any();

        public string Pattern => Segments.Select(x => x.SegmentPath).Join("/");

        public bool HasSpread => _spread != null;

        public string Name { get; set; }
        public string HttpMethod { get; internal set; }

        public RouteHandler Handler { get; set; }
        public int Order { get; set; }

        /// <summary>
        ///     This is only for testing purposes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static JasperRoute For(string url, string httpMethod)
        {
            return new JasperRoute(httpMethod ?? HttpVerbs.GET, url.TrimStart('/'));
        }

        public static ISegment ToParameter(string path, int position)
        {
            if (path == "...") return new Spread(position, null);

            if (path.StartsWith(":"))
            {
                var key = path.Trim(':');
                return new RouteArgument(key, position);
            }

            if (path.StartsWith("{") && path.EndsWith("}"))
            {
                var key = path.TrimStart('{').TrimEnd('}');
                return new RouteArgument(key, position);
            }

            return new Segment(path, position);
        }

        private void validateSegments()
        {
            if (Segments.FirstOrDefault() is Spread)
                throw new InvalidOperationException(
                    $"'{Pattern}' is an invalid route. Cannot use a spread argument as the first segment");

            if (Segments.FirstOrDefault() is RouteArgument)
                throw new InvalidOperationException(
                    $"'{Pattern}' is an invalid route. Cannot use a route argument as the first segment");
        }


        private void setupArgumentsAndSpread()
        {
            _arguments = new Lazy<RouteArgument[]>(() => Segments.OfType<RouteArgument>().ToArray());
            _spread = Segments.OfType<Spread>().SingleOrDefault();

            if (!HasSpread) return;

            if (!Equals(_spread, Segments.Last()))
                throw new ArgumentOutOfRangeException(nameof(Pattern),
                    "The spread parameter can only be the last segment in a route");
        }

        public override string ToString()
        {
            return $"{HttpMethod}: {Pattern}";
        }

        public string ReadRouteDataFromMethodArguments(Expression expression)
        {
            var arguments = MethodCallParser.ToArguments(expression);
            return "/" + Segments.Select(x => x.ReadRouteDataFromMethodArguments(arguments)).Join("/");
        }

        public string ToUrlFromParameters(IDictionary<string, object> parameters)
        {
            return "/" + Segments.Select(x => x.SegmentFromParameters(parameters)).Join("/");
        }

        public string RoutePatternString()
        {
            return "/" + Segments.Select(x => x.RoutePatternPath()).Join("/");
        }

        // TODO -- not tested, not sure this is actually usable
        public RoutePattern BuildRoutePattern()
        {
            return RoutePatternFactory.Parse(RoutePatternString());
        }

    }
}
