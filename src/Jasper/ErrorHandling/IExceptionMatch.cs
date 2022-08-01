using System;

namespace Jasper.ErrorHandling;

public interface IExceptionMatch
{
    string Description { get; }
    Func<Exception, bool> ToFilter();
}

internal class AlwaysMatches : IExceptionMatch
{
    public string Description => "All exceptions";
    public Func<Exception, bool> ToFilter() => _ => true;
}
