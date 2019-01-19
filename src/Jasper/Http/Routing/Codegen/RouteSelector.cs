using System;
using System.Runtime.CompilerServices;
using Jasper.Http.Model;

namespace Jasper.Http.Routing.Codegen
{
    public abstract class RouteSelector
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract RouteHandler Select(string[] segments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Matches(string first, string second)
        {
            return string.Compare(first, second, StringComparison.OrdinalIgnoreCase) == 0;
        }




    }
}