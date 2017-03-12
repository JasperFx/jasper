using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        protected abstract void beforeGeneratingCode();

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

            return writer.Code();
        }
    }
}