using System;
using Baseline;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.ErrorHandling
{
    public class ExceptionTypeMatch<T> : IExceptionMatch where T : Exception
    {
        private readonly Func<T, bool> _filter = e => true;

        public ExceptionTypeMatch(Func<T, bool> filter)
        {
            if (filter != null) _filter = filter;
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            return ex is T && _filter(ex.As<T>());
        }

        public override string ToString()
        {
            return "If the exception is " + typeof(T).Name;
        }
    }
}
