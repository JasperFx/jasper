using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class RequeueContinuation : IContinuation
    {
        public static readonly RequeueContinuation Instance = new RequeueContinuation();

        private RequeueContinuation()
        {
        }

        public ValueTask Execute(IExecutionContext execution, DateTimeOffset now)
        {
            return execution.Defer();
        }

        public override string ToString()
        {
            return "Defer the message for later processing";
        }
    }
}
