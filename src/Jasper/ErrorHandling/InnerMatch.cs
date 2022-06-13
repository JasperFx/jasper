using System;

namespace Jasper.ErrorHandling;

public class InnerMatch : IExceptionMatch
{
    private readonly IExceptionMatch _inner;

    public InnerMatch(IExceptionMatch inner)
    {
        _inner = inner;
    }

    public string Description => $"Inner: " + _inner.Description;
    public Func<Exception, bool> ToFilter()
    {
        var inner = _inner.ToFilter();
        return e => e.InnerException != null && inner(e.InnerException);
    }
}