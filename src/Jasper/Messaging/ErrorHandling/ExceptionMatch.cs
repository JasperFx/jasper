using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.ErrorHandling
{
    public class ExceptionMatch : IExceptionMatch
    {
        private readonly Func<Exception, bool> _filter;

        public ExceptionMatch(Func<Exception, bool> filter, string description)
        {
            _filter = filter;
            Description = description;
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            return _filter(ex);
        }

        public string Description { get; }

        public override string ToString()
        {
            return Description;
        }
    }
}