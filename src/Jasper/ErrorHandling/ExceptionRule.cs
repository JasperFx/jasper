using System;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public record ExceptionRule(Func<Exception, bool> Filter, Func<Envelope, Exception, IContinuation> Action);