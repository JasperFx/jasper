using System.Linq;
using System.Runtime.CompilerServices;
using JasperHttp.Model;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Routing.Codegen
{
    public abstract class RouteSelector
    {


        public RouteHandler Root { get; set; }

        public RouteHandler Select(HttpContext context, out string[] segments)
        {
            if (context.Request.Path == "/")
            {
                segments = RouteTree.Empty;
                return Root;
            }

            segments = ToSegments(context.Request.Path);
            return Select(segments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] ToSegments(string route)
        {
            return route.Split('/').Skip(1).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract RouteHandler Select(string[] segments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Matches(string first, string second)
        {
            return first.Equals(second);
        }




    }
}
