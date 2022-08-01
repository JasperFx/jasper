using System;
using Jasper.Runtime;

namespace Jasper.ErrorHandling.New;

/// <summary>
/// Plugin point for creating continuations based on failures
/// </summary>
public interface IContinuationSource
{
    string Description { get; }
    IContinuation Build(Exception ex, Envelope envelope);
}
