using System;
using System.Collections.Generic;
using System.Linq;
using JasperHttp.Routing.Codegen;
using LamarCodeGeneration.Frames;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Routing
{
    public class Spread : ISegment, IRoutingFrameSource
    {
        public Spread(int position)
        {
            Position = position;
        }

        public Frame ToParsingFrame(MethodCall action)
        {
            var parameter = action.Method.GetParameters().Single(x => x.IsSpread());
            return parameter.Name == Route.PathSegments
                ? (Frame) new PathSegmentsFrame(Position)
                : new RelativePathFrame(Position);
        }

        public int Position { get; }

        public string CanonicalPath()
        {
            return string.Empty;
        }

        public string SegmentPath { get; } = "...";

        public string SegmentFromModel(object model)
        {
            throw new NotSupportedException();
        }

        public string ReadRouteDataFromMethodArguments(List<object> arguments)
        {
            throw new NotSupportedException();
        }

        public string SegmentFromParameters(IDictionary<string, object> parameters)
        {
            throw new NotSupportedException();
        }

        public void SetValues(HttpContext routeData, string[] segments)
        {
            var spreadData = getSpreadData(segments);
            routeData.SetSpreadData(spreadData);
        }

        private string[] getSpreadData(string[] segments)
        {
            if (segments.Length == 0) return new string[0];

            if (Position == 0) return segments;

            if (Position > segments.Length - 1) return new string[0];

            return segments.Skip(Position).ToArray();
        }

        public override string ToString()
        {
            return $"spread:{Position}";
        }

        protected bool Equals(Spread other)
        {
            return Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Spread) obj);
        }

        public override int GetHashCode()
        {
            return Position;
        }
    }
}
