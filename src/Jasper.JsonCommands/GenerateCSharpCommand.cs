using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using LamarCodeGeneration;
using LamarCompiler;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using Oakton;

namespace Jasper.JsonCommands
{
    [Description("Generate C# classes from Json schema files exported by Jasper", Name = "generate-message-types")]
    public class GenerateCSharpCommand : OaktonAsyncCommand<GenerateInput>
    {
        public override async Task<bool> Execute(GenerateInput input)
        {
            var settings = new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                Namespace = input.NamespaceFlag
            };

            var codeDirectory = input.OutputDirectory.ToFullPath();


            var system = new FileSystem();
            if (!system.DirectoryExists(codeDirectory))
            {
                Console.WriteLine("Creating directory " + codeDirectory);
                system.CreateDirectory(codeDirectory);
            }

            var files = system.FindFiles(input.SchemaDirectory, FileSet.Shallow("*.json"));
            var messages = files.Select(x => new GeneratedMessage(x)).ToArray();

            var writer = new SourceWriter();
            writer.Namespace(input.NamespaceFlag);

            foreach (var message in messages)
            {
                var schema = await JsonSchema4.FromFileAsync(message.FilePath);


                var generator = new CSharpGenerator(schema, settings);
                var contents = generator.GenerateFile(message.ClassName);

                var codeFile = codeDirectory.AppendPath(message.ClassName + ".cs");
                Console.WriteLine(codeFile);
                system.WriteStringToFile(codeFile, contents);

                message.WriteAnnotations(writer);
            }

            writer.FinishBlock();

            var annotationFile = codeDirectory.AppendPath("MessageAnnotations.cs");
            Console.WriteLine("Writing attribute annotations to " + annotationFile);
            system.WriteStringToFile(annotationFile, writer.Code());


            return true;
        }
    }
}
