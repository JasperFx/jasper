using System;

namespace Jasper.Codegen
{
    public class CodeGenerationException : Exception
    {
        public CodeGenerationException(object subject, Exception ex) : base($"Error while trying to generate code for '{subject}'", ex)
        {
        }
    }
}