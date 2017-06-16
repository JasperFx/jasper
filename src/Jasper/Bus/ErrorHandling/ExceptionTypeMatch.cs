using System;
using JasperBus.Runtime;

namespace JasperBus.ErrorHandling
{
    public class ExceptionTypeMatch<T> : IExceptionMatch where T : Exception
    {
        public bool Matches(Envelope envelope, Exception ex)
        {
            return ex is T;
        }

        public override string ToString()
        {
            return "If the exception is " + typeof(T).Name;
        }
    }
}