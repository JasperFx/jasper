using System;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Preview the configuration of the Jasper application without running the application or starting Kestrel")]
    public class DescribeCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            using (var runtime = input.BuildRuntime(StartMode.Lightweight))
            {
                runtime.Get<JasperRegistry>().Describe(runtime, Console.Out);
            }

            return true;
        }
    }
}
