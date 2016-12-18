
using System;

namespace Jasper.Codegen.Compilation
{
    public static class SourceWriterExtensions
    {
        public static void Namespace(this ISourceWriter writer, string @namespace)
        {
            writer.Write($"BLOCK:namespace {@namespace}");
        }

        public static void UsingNamespace<T>(this ISourceWriter writer)
        {
            writer.Write($"using {typeof(T).Namespace};");
        }

        public static void UsingBlock(this ISourceWriter writer, string declaration, Action<ISourceWriter> inner)
        {
            writer.Write($"BLOCK:using ({declaration})");

            inner(writer);

            writer.FinishBlock();
        }
    }
}