using System;
using Jasper.CommandLine;
using Oakton;
using Oakton.AspNetCore;

namespace Module1
{
    public class TalkCommand : OaktonCommand<NetCoreInput>
    {
        public override bool Execute(NetCoreInput input)
        {
            ConsoleWriter.Write(ConsoleColor.Magenta, "Hello!");
            return true;
        }
    }
}
