using System;
using System.Net.Http;
using LamarCompiler.Frames;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.MvcExtender
{
    public class BuildOutControllerContextFrame : ConstructorFrame<ControllerContext>
    {
        public BuildOutControllerContextFrame() : base(typeof(ControllerContext).GetConstructor(new Type[0]))
        {
            Set(x => x.HttpContext);
        }
    }
}
