using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Codegen.Compilation;
using StructureMap;

namespace Jasper.Codegen
{
    public abstract class HandlerSet<TChain, THandler>
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
                var @class = chain.ToClass(generation);

                writer.WriteLine($"// START: {chain.TypeName}");
                @class.Write(writer);
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
}
