﻿using System;
using LamarCodeGeneration;

namespace Jasper.Runtime.Routing;

public class NoRoutesException : Exception
{
    public NoRoutesException(Envelope? envelope) : base($"Could not determine any valid routes for {envelope}")
    {
    }

    public NoRoutesException(Type messageType) : base(
        $"Could not determine any valid routes for message type {messageType.FullNameInCode()}")
    {
    }
}
