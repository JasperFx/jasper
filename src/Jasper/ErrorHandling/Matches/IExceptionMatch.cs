using System;

namespace Jasper.ErrorHandling.Matches;

internal interface IExceptionMatch
{
    string Description { get; }

    bool Matches(Exception ex);
}
