using Baseline;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerServiceCreation : Frame
    {
        public ServiceVariable Service { get; }

        public NestedContainerServiceCreation(ServiceVariable service, NestedContainerVariable parent) : base(false)
        {
            Service = service;
            creates.Fill(service);
            uses.Add(parent);
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Service.Usage} = {StructureMapServices.Nested.Usage}.GetInstance<{Service.VariableType.FullNameInCode()}>();");
            Next?.GenerateCode(method, writer);
        }
    }
}