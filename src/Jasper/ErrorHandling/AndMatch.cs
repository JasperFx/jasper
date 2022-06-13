using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.ErrorHandling;

public class AndMatch : IExceptionMatch
{
    public readonly List<IExceptionMatch> Inners = new();

    public AndMatch(params IExceptionMatch[] matches)
    {
        Inners.AddRange(matches);
    }

    public string Description => Inners.Select(x => ExceptionMatchExtensions.Formatted(x)).Join(" and ");
    public Func<Exception, bool> ToFilter()
    {
        var filters = Inners.Select(x => x.ToFilter()).ToArray();
        return e => filters.All(x => x(e));
    }
}