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

        public string Description { get; }

        public bool Matches(Envelope envelope, Exception ex)
        {
            return _filter(ex);
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
