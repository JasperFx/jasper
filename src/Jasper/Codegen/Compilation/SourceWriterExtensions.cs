namespace Jasper.Codegen.Compilation
{
    public static class SourceWriterExtensions
    {
        public static void Namespace(this ISourceWriter writer, string @namespace)
        {
            writer.Write($"BLOCK:namespace {@namespace}");
        }

        
    }
}