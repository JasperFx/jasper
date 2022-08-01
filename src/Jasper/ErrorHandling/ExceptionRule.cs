using System;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

[Obsolete]
public record ExceptionRule(Func<Exception, bool> Filter, Func<Envelope, Exception, IContinuation> Action);
