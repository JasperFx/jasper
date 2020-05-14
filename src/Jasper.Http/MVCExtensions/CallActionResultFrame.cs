using System;
using LamarCodeGeneration.Frames;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Http.MVCExtensions
{
    public class CallActionResultFrame : MethodCall
    {
        public CallActionResultFrame(Type handlerType) : base(handlerType, nameof(IActionResult.ExecuteResultAsync))
        {
        }
    }
}
