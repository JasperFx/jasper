using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Routing
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSpread(this ParameterInfo parameter)
        {
            if (parameter.Name == Route.RelativePath && parameter.ParameterType == typeof(string)) return true;
            if (parameter.Name == Route.PathSegments && parameter.ParameterType == typeof(string[])) return true;
            return false;
        }
    }

    public class Route
    {
        public const string RelativePath = "relativePath";
        public const string PathSegments = "pathSegments";

        /// <summary>
        /// This is only for testing purposes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Route For(string url, string httpMethod)
        {
            return new Route(url, httpMethod ?? HttpVerbs.GET);
        }

        public static ISegment ToParameter(string path, int position)
        {
            if (path == "...")
            {
                return new Spread(position);
            }

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

        private Lazy<RouteArgument[]> _arguments;
        private Spread _spread;
        private readonly List<ISegment> _segments = new List<ISegment>();


        public Route(string pattern, string httpMethod)
        {
            pattern = pattern?.TrimStart('/').TrimEnd('/') ?? throw new ArgumentNullException(nameof(pattern));


            HttpMethod = httpMethod;

            var segments = pattern.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = ToParameter(segments[i], i);
                _segments.Add(segment);
            }

            validateSegments();



            Pattern = string.Join("/", _segments.Select(x => x.SegmentPath));

            Name = $"{HttpMethod}:{Pattern}";

            setupArgumentsAndSpread();
        }

        private void validateSegments()
        {
            if (_segments.FirstOrDefault() is Spread)
            {
                throw new InvalidOperationException($"'{Pattern}' is an invalid route. Cannot use a spread argument as the first segment");
            }

            if (_segments.FirstOrDefault() is RouteArgument)
            {
                throw new InvalidOperationException($"'{Pattern}' is an invalid route. Cannot use a route argument as the first segment");
            }
        }

        public Route(ISegment[] segments, string httpVerb)
        {
            _segments.AddRange(segments);

            validateSegments();

            HttpMethod = httpVerb;

            Pattern = _segments.Select(x => x.SegmentPath).Join("/");
            Name = $"{HttpMethod}:{Pattern}";

            setupArgumentsAndSpread();
        }



        private void setupArgumentsAndSpread()
        {
            _arguments = new Lazy<RouteArgument[]>(() => _segments.OfType<RouteArgument>().ToArray());
            _spread = _segments.OfType<Spread>().SingleOrDefault();

            if (!HasSpread) return;

            if (!Equals(_spread, _segments.Last()))
                throw new ArgumentOutOfRangeException(nameof(Pattern),
                    "The spread parameter can only be the last segment in a route");
        }



        public IEnumerable<ISegment> Segments => _segments;

        public Type InputType { get; set; }
        public Type HandlerType { get; set; }
        public MethodInfo Method { get; set; }



        public RouteArgument GetArgument(string key)
        {
            return _segments.OfType<RouteArgument>().FirstOrDefault(x => x.Key == key);
        }

        public bool EndsWithArgument
        {
            get
            {
                if (_segments.LastOrDefault() is RouteArgument)
                {
                    return true;
                }

                if (_segments.LastOrDefault() is Spread && _segments.Count >= 2)
                {
                    return _segments[_segments.Count - 2] is RouteArgument;
                }

                return false;
            }
        }

        public bool HasParameters => HasSpread || _arguments.Value.Any();

        public IEnumerable<RouteArgument> Arguments => _arguments.Value.ToArray();

        public string Pattern { get; }

        public bool HasSpread => _spread != null;

        public string Name { get; set; }
        public string HttpMethod { get; }

        public string NodePath
        {
            get
            {
                var segments = _segments.ToArray().Reverse().Skip(1).Reverse().ToArray();

                if (HasSpread && EndsWithArgument)
                {
                    segments = _segments.ToArray().Reverse().Skip(2).Reverse().ToArray();
                }

                return string.Join("/", segments.Select(x => x.CanonicalPath()));
            }
        }

        public string LastSegment => _segments.Count == 0 ? string.Empty : _segments.Last().CanonicalPath();

        public IEnumerable<ISegment> Parameters => _segments.Where(x => !(x is Segment)).ToArray();
        public RouteHandler Handler { get; set; }

        [Obsolete("Don't wanna use this in real life")]
        public void SetValues(HttpContext context, string[] segments)
        {
            if (HasParameters)
            {
                var routeData = new Dictionary<string, object>();
                _arguments.Value.Each(x => x.SetValues(routeData, segments));

                context.SetRouteData(routeData);
            }

            if (HasSpread)
            {
                _spread.SetValues(context, segments);
            }
        }


        public IDictionary<string, string> ToParameters(object input)
        {
            var dict = new Dictionary<string, string>();
            _arguments.Value.Each(x => dict.Add(x.Key, x.ReadRouteDataFromInput(input)));

            return dict;
        }


        public void WriteToInputModel(object model, Dictionary<string, object> dict)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (model.GetType() != InputType) throw new ArgumentOutOfRangeException(nameof(model), $"This route maps to {InputType} but got {model.GetType()}");


            _arguments.Value.Each(x => x.ApplyRouteDataToInput(model, dict));
        }

        public string ToUrlFromInputModel(object model)
        {
            return "/" + _segments.Select(x => x.SegmentFromModel(model)).Join("/");
        }

        public override string ToString()
        {
            return $"{HttpMethod}:{Pattern}";
        }

        public string ReadRouteDataFromMethodArguments(Expression expression)
        {
            var arguments = MethodCallParser.ToArguments(expression);
            return "/" + _segments.Select(x => x.ReadRouteDataFromMethodArguments(arguments)).Join("/");
        }

        public string ToUrlFromParameters(IDictionary<string, object> parameters)
        {
            return "/" + _segments.Select(x => x.SegmentFromParameters(parameters)).Join("/");
        }



    }
}
