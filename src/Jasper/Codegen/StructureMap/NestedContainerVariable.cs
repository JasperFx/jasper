using System;
using System.Collections.Generic;
using Jasper.Codegen.Compilation;
using StructureMap;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerVariable : Variable
    {
        public NestedContainerVariable() : base(typeof(IContainer), "nested", VariableCreation.BuiltByFrame)
        {
        }

        public override IEnumerable<Variable> Dependencies
        {
            get { yield return StructureMapServices.Root; }
        }

        protected override void generateCreation(ISourceWriter writer, Action<ISourceWriter> continuation)
        {
            writer.UsingBlock("var nested = _root.GetNestedContainer()", continuation);
        }
    }
}