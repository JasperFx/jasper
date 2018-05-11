using System;
using Baseline;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports.Configuration;
using Lamar.Codegen;
using Lamar.Compilation;
using Oakton;

namespace Jasper.CommandLine
{
    [Description("Display or export the runtime, generated code in this application")]
    public class CodeCommand : OaktonCommand<CodeInput>
    {
        public CodeCommand()
        {
            Usage("Show everything in the console").Arguments();
            Usage("Show selected code in the console").Arguments(x => x.Match);
        }

        public override bool Execute(CodeInput input)
        {
            Console.WriteLine("Generating a preview of the generated code, this might take a bit...");
            Console.WriteLine();
            Console.WriteLine();

            input.Registry.Settings.Alter<MessagingSettings>(x => x.HostedServicesEnabled = false);
            var runtime = input.BuildRuntime();

            var rules = runtime.Get<GenerationRules>();
            var generatedAssembly = new GeneratedAssembly(rules);

            if (input.Match == CodeMatch.all || input.Match == CodeMatch.messages)
            {

                var handlers = runtime.Get<HandlerGraph>();
                foreach (var handler in handlers.Chains)
                {
                    handler.AssembleType(generatedAssembly);
                }
            }

            if (input.Match == CodeMatch.all || input.Match == CodeMatch.routes)
            {
                var connegRules = runtime.Get<ConnegRules>();
                var routes = runtime.Get<RouteGraph>();

                foreach (var route in routes)
                {
                    route.AssemblyType(generatedAssembly, connegRules);
                }
            }

            var text = runtime.Container.GenerateCodeWithInlineServices(generatedAssembly);

            Console.WriteLine(text);

            if (input.FileFlag.IsNotEmpty())
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"Writing file {input.FileFlag.ToFullPath()}");
                new FileSystem().WriteStringToFile(input.FileFlag, text);
            }

            return true;
        }
    }

    public enum CodeMatch
    {
        all,
        messages,
        routes
    }

    public class CodeInput : JasperInput
    {
        public CodeMatch Match { get; set; } = CodeMatch.all;

        [System.ComponentModel.Description("Optional file name to export the contents")]
        public string FileFlag { get; set; }
    }
}
