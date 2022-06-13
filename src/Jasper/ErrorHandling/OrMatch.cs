using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.ErrorHandling;

public class OrMatch : IExceptionMatch
{
    public readonly List<IExceptionMatch> Inners = new();

    public OrMatch(params IExceptionMatch[] matches)
    {
        Inners.AddRange(matches);
    }

    public string Description => Inners.Select(x => ExceptionMatchExtensions.Formatted(x)).Join(" or ");
    public Func<Exception, bool> ToFilter()
    {
        var filters = Inners.Select(x => x.ToFilter()).ToArray();
        return e => filters.Any(x => x(e));
    }
}