using System;
using System.Diagnostics;
using LamarCodeGeneration;

namespace Jasper.Persistence.Sagas;

public class IndeterminateSagaStateIdException : Exception
{
    public IndeterminateSagaStateIdException(Envelope envelope) : base(
        $"Could not determine a valid saga state id for Envelope {envelope}")
    {
    }
}

public class UnknownSagaStateException : Exception
{
    public UnknownSagaStateException(Type sagaStateType, object stateId) : base(
        $"Could not find an expected state document of type {sagaStateType.FullNameInCode()} for id '{stateId}'")
    {
        Debug.WriteLine("something");
    }
}
