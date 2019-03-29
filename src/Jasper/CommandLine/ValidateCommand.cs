using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Messaging.Model;
using Lamar;
using LamarCodeGeneration;
using LamarCompiler;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Validate the configuration and environment for this Jasper application")]
    public class ValidateCommand : OaktonAsyncCommand<JasperInput>
    {
        public override async Task<bool> Execute(JasperInput input)
        {
            Console.WriteLine("Bootstrapping the system and running all checks...");
            using (var host = input.BuildHost(StartMode.Lightweight))
            {
                Console.WriteLine("Validating the Lamar configuration and executing all Lamar environment checks");
                host.Container.AssertConfigurationIsValid(AssertMode.Full);

                Console.WriteLine("Generating code for all the message handlers, this might take a bit...");
                Console.WriteLine();
                Console.WriteLine();

                var rules = host.Get<JasperGenerationRules>();
                var generatedAssembly = new GeneratedAssembly(rules);
                var handlers = host.Get<HandlerGraph>();
                foreach (var handler in handlers.Chains) handler.AssembleType(generatedAssembly, rules);



                Console.WriteLine();
                Console.WriteLine("Trying to compile the routes...");

                await host.Get<HttpSettings>().BuildRouting(host.Container, host.Get<JasperGenerationRules>());


                Console.WriteLine();
                Console.WriteLine("Trying to run the environment checks...");

                host.ExecuteAllEnvironmentChecks();
            }

            ConsoleWriter.Write(ConsoleColor.Green, "All systems good!");

            return true;
        }
    }
}
