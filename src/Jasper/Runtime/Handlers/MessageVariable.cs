﻿using System;
using LamarCodeGeneration.Model;

namespace Jasper.Runtime.Handlers;

public class MessageVariable : Variable
{
    public MessageVariable(Variable envelope, Type messageType) : base(messageType, DefaultArgName(messageType))
    {
        Creator = new MessageFrame(this, envelope);
    }
}
