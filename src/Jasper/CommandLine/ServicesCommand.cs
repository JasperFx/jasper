using System;
using Oakton;

namespace Jasper.CommandLine
{
    // SAMPLE: ServicesCommand
    [Description("Display the known Lamar service registrations")]
    public class ServicesCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            using (var runtime = input.BuildHost(StartMode.Lightweight))
            {
                Console.WriteLine(runtime.Container.WhatDoIHave());
            }

            return true;
        }
    }

    // ENDSAMPLE
}
