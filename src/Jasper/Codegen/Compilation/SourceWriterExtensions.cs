
namespace Jasper.Codegen.Compilation
{
    public static class SourceWriterExtensions
    {
        public static void Namespace(this ISourceWriter writer, string @namespace)
        {
            writer.Write($"BLOCK:namespace {@namespace}");
        }

        public static void Using<T>(this ISourceWriter writer)
        {
            writer.Write($"using {typeof(T).Namespace};");
        }
    }
}