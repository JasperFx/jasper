using Oakton;

namespace Jasper.JsonCommands
{
    public class GenerateInput
    {
        [Description("The directory where the Json schema files are located")]
        public string SchemaDirectory { get; set; }

        [Description("The directory where the generated C# files should be written")]
        public string OutputDirectory { get; set; }

        [Description("Override the namespace of the generated message types")]
        public string NamespaceFlag { get; set; } = "Jasper.MessageTypes";
    }
}