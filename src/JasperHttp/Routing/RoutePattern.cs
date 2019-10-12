using System.Linq;
using Baseline;

namespace JasperHttp.Routing
{
    public class RoutePattern
    {
        public RoutePattern(string method, string pattern)
        {
            Method = method;
            Pattern = pattern;

            if (pattern == "/" || pattern.IsEmpty())
            {
                Segments = new Segment[0];
            }
            else
            {
                var parts = pattern.TrimStart('/').Split('/');
                Segments = parts
                    .Select((x, position) => new Segment(x.ToLowerInvariant(), position))
                    .OfType<ISegment>()
                    .ToArray();
            }
        }

        public RoutePattern(string method, string pattern, ISegment[] segments)
        {
            Method = method;
            Pattern = pattern;
            Segments = segments;
        }

        public string Method { get; set; }
        public string Pattern { get; set; }
        public ISegment[] Segments { get; private set; }
        public int Order { get; set; }
    }
}
