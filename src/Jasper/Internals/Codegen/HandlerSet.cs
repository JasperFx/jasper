using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Internals.Compilation;

namespace Jasper.Internals.Codegen
{
    public static class ServiceProviderExtensions
    {
        public static object Build(this IServiceProvider services, Type type)
        {
            var ctor = type.GetTypeInfo().GetConstructors().Single();
            var inputs = ctor.GetParameters().Select(x => services.GetService(x.ParameterType)).ToArray();
            return Activator.CreateInstance(type, inputs);
        }
    }

    public abstract class HandlerSet<TChain, THandler>
        where TChain : class, IGenerates<THandler>

    {
        protected abstract TChain[] chains { get; }

        public THandler[] CompileAndBuildAll(GenerationRules generation, Func<Type, object> builder)
        {
            var types = CompileAll(generation);
            return chains.Select(x => x.Create(types, builder)).ToArray();
        }

        public Type[] CompileAll(GenerationRules generation)
        {
            var code = GenerateCode(generation);

            var generator = buildGenerator(generation);

            var assembly = generator.Generate(code);

            return assembly.GetExportedTypes().ToArray();
        }

        protected virtual void beforeGeneratingCode()
        {

        }

        private AssemblyGenerator buildGenerator(GenerationRules generation)
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

        public string GenerateCode(GenerationRules generation)
        {
            beforeGeneratingCode();

            var classes = chains.Select(x => x.ToClass(generation)).ToArray();
            var namespaces = classes
                .SelectMany(x => x.Args())
                .Select(x => x.ArgType.Namespace)
                .Concat(new string[]{typeof(Task).Namespace})
                .Distinct().ToList();

            var writer = new SourceWriter();

            foreach (var ns in namespaces.OrderBy(x => x))
            {
                writer.Write($"using {ns};");
            }

            writer.BlankLine();

            writer.Namespace(generation.ApplicationNamespace);

            foreach (var @class in classes)
            {
                writer.WriteLine($"// START: {@class.ClassName}");
                @class.Write(writer);
                writer.WriteLine($"// END: {@class.ClassName}");

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
                chain.SourceCode = parser.CodeFor(chain.TypeName);
            }
        }
    }
}
