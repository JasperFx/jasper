using System;
using LamarCodeGeneration;

namespace JasperHttp.Routing.Codegen
{
    public static class RoutingSourceWriterExtensions
    {

        public static void IfCurrentSegmentEquals(this ISourceWriter writer, int position, string routeSegment, Action<ISourceWriter> inner)
        {
            writer.Write($"BLOCK:if ({nameof(RouteSelector.Matches)}({RoutingFrames.Segments}[{position}], \"{routeSegment}\"))");
            inner(writer);
            writer.FinishBlock();
        }

        public static void Return(this ISourceWriter writer, Route route)
        {
            writer.WriteComment(route.Description);
            writer.Write($"return {route.VariableName};");
        }

        public static void ReturnNull(this ISourceWriter writer)
        {
            writer.Write("return null;");
        }
    }
}
