using System;
using System.Collections.Generic;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen.StructureMap
{
    public class ServiceVariable : Variable
    {
        private readonly NestedContainerVariable _parent;

        public ServiceVariable(Type argType, NestedContainerVariable parent) : base(argType, VariableCreation.BuiltByFrame)
        {
            _parent = parent;
        }

        public override IEnumerable<Variable> Dependencies
        {
            get
            {
                yield return _parent;
            }
        }


        protected override void generateCreation(ISourceWriter writer, Action<ISourceWriter> continuation)
        {
            writer.Write($"var {Name} = {StructureMapServices.Nested.Name}.GetInstance<{VariableType.FullName}>();");
            continuation(writer);
        }
    }
}