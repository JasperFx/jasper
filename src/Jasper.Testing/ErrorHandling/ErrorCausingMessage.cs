using System;
using System.Collections.Generic;

namespace Jasper.Testing.ErrorHandling
{
    public class ErrorCausingMessage
    {
        public Dictionary<int, Exception> Errors = new Dictionary<int, Exception>();
        public bool WasProcessed { get; set; }
        public int LastAttempt { get; set; }
    }
}