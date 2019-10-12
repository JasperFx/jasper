using System.Collections.Generic;

namespace JasperHttp.Routing
{
    public interface ISegment
    {
        int Position { get; }

        string SegmentPath { get; }
        string CanonicalPath();
        string SegmentFromModel(object model);

        string ReadRouteDataFromMethodArguments(List<object> arguments);
        string SegmentFromParameters(IDictionary<string, object> parameters);
    }
}
