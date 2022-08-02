using System;
using LamarCodeGeneration;

namespace Jasper.ErrorHandling;

public class TypeMatch<T> : IExceptionMatch where T : Exception
{
    public string Description => "Exception is " + typeof(T).FullNameInCode();

    public bool Matches(Exception ex)
    {
        return ex is T;
    }
}
