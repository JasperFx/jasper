using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Http.Model
{
    public class DuplicateRoutesException : Exception
    {
        public DuplicateRoutesException(IEnumerable<RouteChain> chains) : base(
            $"Duplicated route with pattern {chains.First().Route.Name} between {chains.Select(x => $"{x.Action}").Join(", ")}")
        {
        }
    }
}
