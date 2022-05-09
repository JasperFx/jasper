using System;

namespace Jasper.Persistence.Sagas;

/// <summary>
///     Marks a public property on a message type as the saga state identity
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SagaIdentityAttribute : Attribute
{
}
