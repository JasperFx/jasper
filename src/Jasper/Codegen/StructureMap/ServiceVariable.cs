using System;
using Baseline;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerServiceCreation : Frame
    {
        public ServiceVariable Service { get; }

        public NestedContainerServiceCreation(ServiceVariable service) : base(false)
        {
            Service = service;
            creates.Fill(service);
        }

        public override void GenerateCode(IHandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"var {Service.Usage} = {StructureMapServices.Nested.Usage}.GetInstance<{Service.VariableType.FullName}>();");
        }
    }
    
    public class ServiceVariable : Variable
    {
        private readonly NestedContainerVariable _parent;

        public ServiceVariable(Type argType, NestedContainerVariable parent) : base(argType, DefaultArgName(argType))
        {
            Dependencies.Add(parent);
            Creator = new NestedContainerServiceCreation(this);
        }
    }
}