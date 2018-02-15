using System;
using Jasper.Messaging.Transports.Configuration;
using Oakton;

namespace Jasper.CommandLine
{
    // SAMPLE: ServicesCommand
    [Description("Display the known StructureMap service registrations")]
    public class ServicesCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            input.Registry.Settings.Alter<MessagingSettings>(_ =>
            {
                _.ThrowOnValidationErrors = false;
            });

            using (var runtime = input.BuildRuntime())
            {
                Console.WriteLine(runtime.Container.WhatDoIHave());
            }

            return true;
        }
    }
    // ENDSAMPLE
}
