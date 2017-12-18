using System;
using Jasper.CommandLine;
using Oakton;

namespace Module1
{
    public class TalkCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            ConsoleWriter.Write(ConsoleColor.Magenta, "Hello!");
            return true;
        }
    }
}
