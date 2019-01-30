using System;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Messaging.Model;
using Lamar;
using LamarCompiler;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Validate the configuration and environment for this Jasper application")]
    public class ValidateCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            Console.WriteLine("Bootstrapping the system and running all checks...");
            using (var runtime = input.BuildHost(StartMode.Lightweight))
            {
                Console.WriteLine("Validating the Lamar configuration and executing all Lamar environment checks");
                runtime.Container.AssertConfigurationIsValid(AssertMode.Full);

                Console.WriteLine("Generating code for all the message handlers, this might take a bit...");
                Console.WriteLine();
                Console.WriteLine();

                var rules = runtime.Get<JasperGenerationRules>();
                var generatedAssembly = new GeneratedAssembly(rules);
                var handlers = runtime.Get<HandlerGraph>();
                foreach (var handler in handlers.Chains) handler.AssembleType(generatedAssembly, rules);



                Console.WriteLine();
                Console.WriteLine("Trying to compile the routes...");

                var router = runtime.Get<HttpSettings>().BuildRouting(runtime.Container, runtime.Get<JasperGenerationRules>())
                    .GetAwaiter().GetResult();



            }

            ConsoleWriter.Write(ConsoleColor.Green, "All systems good!");

            return true;
        }
    }
}
