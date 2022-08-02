using System;

namespace Jasper.ErrorHandling;

public interface IExceptionMatch
{
    string Description { get; }

    bool Matches(Exception ex);
}

internal class AlwaysMatches : IExceptionMatch
{
    public string Description => "All exceptions";

    public bool Matches(Exception ex)
    {
        return true;
    }
}
