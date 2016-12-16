using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class HandlerSet<TInput, THandlerChain> where THandlerChain : HandlerChain
    {
        private readonly string _inputArgName;
        public GenerationConfig Config { get; set; }
        private readonly IList<THandlerChain> _chains = new List<THandlerChain>();

        public HandlerSet(GenerationConfig config, string inputArgName)
        {
            _inputArgName = inputArgName;
            Config = config;
        }

        public void Add(THandlerChain chain)
        {
            _chains.Add(chain);
        }

        public Type[] CompileAll()
        {
            var code = generateCode();

            var generator = buildGenerator();

            var assembly = generator.Generate(code);

            return assembly.GetExportedTypes().ToArray();
        }

        private AssemblyGenerator buildGenerator()
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(GetType().GetTypeInfo().Assembly);
            foreach (var assembly in Config.Assemblies)
            {
                generator.ReferenceAssembly(assembly);
            }
            return generator;
        }

        private string generateCode()
        {
            var writer = new SourceWriter();

            writer.Namespace(Config.ApplicationNamespace);

            foreach (var chain in _chains)
            {
                var generation = new HandlerGeneration(Config, typeof(TInput), _inputArgName);
                chain.Write(generation, writer);

                writer.WriteLine("");
                writer.WriteLine("");
            }

            writer.FinishBlock();

            return writer.Code();
        }
    }
}