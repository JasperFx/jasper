using System;

namespace Jasper.ErrorHandling;

public interface IExceptionMatch
{
    string Description { get; }
    Func<Exception, bool> ToFilter();
}
