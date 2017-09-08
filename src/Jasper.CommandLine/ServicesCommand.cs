using System;
using Jasper.Bus;
using Jasper.Bus.Settings;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Display the known StructureMap service registrations")]
    public class ServicesCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            input.Registry.Settings.Alter<BusSettings>(_ =>
            {
                _.ThrowOnValidationErrors = false;
            });

            using (var runtime = input.BuildRuntime())
            {
                Console.WriteLine(runtime.Container.WhatDoIHave());
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(runtime.Container.WhatDidIScan());
            }

            return true;
        }
    }
}