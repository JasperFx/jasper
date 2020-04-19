using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Http.Routing.Codegen;
using LamarCodeGeneration.Frames;

namespace Jasper.Http.Routing
{
    public class Spread : ISegment, IRoutingFrameSource
    {
        public Spread(int position, ParameterInfo parameter)
        {
            Position = position;
            Parameter = parameter;
        }

        public Frame ToParsingFrame(MethodCall action)
        {
            var parameter = action.Method.GetParameters().Single(x => x.IsSpread());
            return parameter.Name == JasperRoute.PathSegments
                ? (Frame) new PathSegmentsFrame(Position)
                : new RelativePathFrame(Position);
        }



        public int Position { get; }
        public ParameterInfo Parameter { get; }

        public string SegmentPath { get; } = "...";

        public string ReadRouteDataFromMethodArguments(List<object> arguments)
        {
            throw new NotSupportedException();
        }

        public string SegmentFromParameters(IDictionary<string, object> parameters)
        {
            throw new NotSupportedException();
        }

        public string RoutePatternPath()
        {
            string[] items = new string[12];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = "{s" + i + "?}";
            }

            return items.Join("/");
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
