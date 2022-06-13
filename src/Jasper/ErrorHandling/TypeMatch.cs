using System;
using LamarCodeGeneration;

namespace Jasper.ErrorHandling;

public class TypeMatch<T> : IExceptionMatch where T : Exception
{
    public string Description => "Exception is " + typeof(T).FullNameInCode();
    public Func<Exception, bool> ToFilter()
    {
        return ex => ex is T;
    }
}