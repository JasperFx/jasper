﻿using System.Reflection;
using Baseline.Reflection;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Runtime.Handlers;

public class CaptureCascadingMessages : MethodCall
{
    private static readonly MethodInfo _method =
#pragma warning disable CS8625
        ReflectionHelper.GetMethod<IExecutionContext>(x => x.EnqueueCascadingAsync(null));
#pragma warning restore CS8625


    public CaptureCascadingMessages(Variable messages) : base(typeof(IExecutionContext),
        _method)
    {
        Arguments[0] = messages;
        CommentText = "Outgoing, cascaded message";
    }
}
