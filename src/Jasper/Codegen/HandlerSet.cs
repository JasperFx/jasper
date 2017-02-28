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
        public GenerationConfig Config { get; set; }

        public HandlerSet(GenerationConfig config)
        {
            Config = config;
        }


        protected abstract TChain[] chains { get; }

        public THandler[] CompileAndBuildAll(IContainer container)
        {
            var types = CompileAll();
            return chains.Select(x => x.Create(types, container)).ToArray();
        }

        public Type[] CompileAll()
        {
            var code = GenerateCode();

            var generator = buildGenerator();

            var assembly = generator.Generate(code);

            return assembly.GetExportedTypes().ToArray();
        }

        private AssemblyGenerator buildGenerator()
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(GetType().GetTypeInfo().Assembly);
            generator.ReferenceAssembly(typeof(Task).GetTypeInfo().Assembly);

            foreach (var assembly in Config.Assemblies)
            {
                generator.ReferenceAssembly(assembly);
            }

            return generator;
        }

        public string GenerateCode()
        {
            var writer = new SourceWriter();

            writer.UsingNamespace<Task>();
            writer.BlankLine();

            writer.Namespace(Config.ApplicationNamespace);

            foreach (var chain in chains)
            {
                var generation = chain.ToHandlerCode(Config);


                // TODO -- figure out how to get the source code for each handler
                writer.WriteLine($"// START: {chain.TypeName}");

                HandlerSourceWriter.Write(generation, writer);
                writer.WriteLine($"// END: {chain.TypeName}");

                writer.WriteLine("");
                writer.WriteLine("");
            }

            writer.FinishBlock();

            return writer.Code();
        }
    }
}