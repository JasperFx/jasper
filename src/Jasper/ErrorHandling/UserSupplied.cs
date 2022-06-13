using System;
using LamarCodeGeneration;

namespace Jasper.ErrorHandling;

public class UserSupplied : IExceptionMatch
{
    private readonly Func<Exception, bool> _filter;

    public UserSupplied(Func<Exception, bool> filter)
    {
        _filter = filter;
        Description = "User supplied filter";
    }

    public UserSupplied(Func<Exception, bool> filter, string description)
    {
        _filter = filter;
        Description = description;
    }

    public string Description { get; }
    public Func<Exception, bool> ToFilter() => _filter;
}

public class UserSupplied<T> : IExceptionMatch where T : Exception
{
    private readonly Func<T, bool> _filter;

    public UserSupplied(Func<T, bool> filter)
    {
        _filter = filter;
        Description = "User supplied filter on " + typeof(T).FullNameInCode();
    }

    public UserSupplied(Func<T, bool> filter, string description)
    {
        _filter = filter;
        Description = description;
    }

    public string Description { get; }
    public Func<Exception, bool> ToFilter() => ex => ex is T matched && _filter(matched);
}
