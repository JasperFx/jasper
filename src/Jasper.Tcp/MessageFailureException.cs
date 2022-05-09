﻿using System;
using System.Linq;
using Baseline;

namespace Jasper.Tcp;

public class MessageFailureException : Exception
{
    public MessageFailureException(Envelope[] messages, Exception innerException) : base(
        $"SEE THE INNER EXCEPTION -- Failed on messages {messages.Select(x => x.ToString()).Join(", ")}",
        innerException)
    {
    }
}
