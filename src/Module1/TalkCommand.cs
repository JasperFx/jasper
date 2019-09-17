using System;
using Jasper.CommandLine;
using Oakton;
using Oakton.AspNetCore;

namespace Module1
{
    public class TalkCommand : OaktonCommand<AspNetCoreInput>
    {
        public override bool Execute(AspNetCoreInput input)
        {
            ConsoleWriter.Write(ConsoleColor.Magenta, "Hello!");
            return true;
        }
    }
}
