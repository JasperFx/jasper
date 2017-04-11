using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen.Compilation;
using Jasper.Codegen.New;
using Jasper.Internal;
using StructureMap;

namespace Jasper.Codegen
{
    public abstract class HandlerSet<TChain, TInput, THandler>
        where THandler : IHandler<TInput>
        where TChain : IGenerates<THandler>

    {
        protected abstract TChain[] chains { get; }

        public THandler[] CompileAndBuildAll(IGenerationConfig generation, IContainer container)
        {
            var types = CompileAll(generation);
            return chains.Select(x => x.Create(types, container)).ToArray();
        }

        public Type[] CompileAll(IGenerationConfig generation)
        {

            var code = GenerateCode(generation);

            var generator = buildGenerator(generation);

            var assembly = generator.Generate(code);

            return assembly.GetExportedTypes().ToArray();
        }

        protected virtual void beforeGeneratingCode()
        {

        }

        private AssemblyGenerator buildGenerator(IGenerationConfig generation)
        {
            // TODO -- should probably do a lot more here. See GH-6
            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(GetType().GetTypeInfo().Assembly);
            generator.ReferenceAssembly(typeof(Task).GetTypeInfo().Assembly);

            foreach (var assembly in generation.Assemblies)
            {
                generator.ReferenceAssembly(assembly);
            }

            return generator;
        }

        public string GenerateCode(IGenerationConfig generation)
        {
            beforeGeneratingCode();

            var writer = new SourceWriter();

            writer.UsingNamespace<Task>();
            writer.BlankLine();

            writer.Namespace(generation.ApplicationNamespace);

            foreach (var chain in chains)
            {
                var generationModel = chain.ToGenerationModel(generation);


                // TODO -- figure out how to get the source code for each handler
                writer.WriteLine($"// START: {chain.TypeName}");

                HandlerSourceWriter.Write(generationModel, writer);
                writer.WriteLine($"// END: {chain.TypeName}");

                writer.WriteLine("");
                writer.WriteLine("");
            }

            writer.FinishBlock();


            var code = writer.Code();

            attachSourceCodeToChains(code);


            return code;
        }

        private void attachSourceCodeToChains(string code)
        {
            var parser = new SourceCodeParser(code);
            foreach (var chain in chains)
            {
                chain.SourceCode = parser.Code[chain.TypeName];
            }
        }
    }

    public class SourceCodeParser
    {
        public readonly LightweightCache<string, string> Code = new LightweightCache<string, string>(name => "UNKNOWN");

        private StringWriter _current;
        private string _name;

        public SourceCodeParser(string code)
        {
            foreach (var line in code.ReadLines())
            {
                if (_current == null)
                {
                    if (line.IsEmpty()) continue;

                    if (line.Trim().StartsWith("// START"))
                    {
                        _name = line.Split(':').Last().Trim();
                        _current = new StringWriter();
                    }
                }
                else
                {
                    if (line.Trim().StartsWith("// END"))
                    {
                        var classCode = _current.ToString();
                        Code[_name] = classCode;

                        _current = null;
                        _name = null;
                    }
                    else
                    {
                        _current.WriteLine(line);
                    }
                }

            }
        }
    }
}
