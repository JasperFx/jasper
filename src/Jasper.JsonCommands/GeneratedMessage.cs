using System.IO;
using System.Linq;
using Baseline;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Util;
using LamarCodeGeneration;
using LamarCompiler;

namespace Jasper.JsonCommands
{
    public class GeneratedMessage
    {
        public GeneratedMessage(string file)
        {
            FilePath = file;
            var name = Path.GetFileNameWithoutExtension(file);
            var parts = name.Split('-');
            Version = parts.Length > 1 ? parts.Last() : "V1";
            MessageAlias = parts.Take(parts.Length - 1).Join("-");
            ClassName = MessageAlias.Split('.').Last();
        }

        public string FilePath { get; }
        public string ClassName { get; }

        public string MessageAlias { get; }

        public string Version { get; }

        public void WriteAnnotations(ISourceWriter writer)
        {
            writer.WriteLine(
                $"[{typeof(MessageIdentityAttribute).FullName.TrimEnd("Attribute".ToCharArray())}(\"{MessageAlias}\")]");
            writer.WriteLine($"public partial class {ClassName} {{}}");
            writer.BlankLine();
        }
    }
}
