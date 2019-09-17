using System;
using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.AspNetCore;

namespace Jasper.CommandLine
{
    [Description("Preview the configuration of the Jasper application without running the application or starting Kestrel")]
    public class DescribeCommand : OaktonCommand<AspNetCoreInput>
    {
        public override bool Execute(AspNetCoreInput input)
        {
            using (var runtime = new JasperRuntime(input.BuildHost()))
            {
                runtime.Get<JasperRegistry>().Describe(runtime, Console.Out);
            }

            return true;
        }
    }
}
