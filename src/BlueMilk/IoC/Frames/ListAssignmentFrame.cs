using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using BlueMilk.Compilation;
using BlueMilk.IoC.Enumerables;

namespace BlueMilk.IoC.Frames
{
    public class ListAssignmentFrame<T> : Frame
    {
        public ListAssignmentFrame(ListInstance<T> instance, Variable[] elements) : base(false)
        {
            ElementType = typeof(T);
            Variable = new ServiceVariable(instance, this);

            Elements = elements;
        }

        public Type ElementType { get; }

        public Variable[] Elements { get; }

        public Variable Variable { get; }
        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var elements = Elements.Select(x => x.Usage).Join(", ");
            if (ReturnCreated)
            {
                writer.Write($"return new {typeof(List<>).Namespace}.List<{ElementType.FullNameInCode()}>{{{elements}}};");
            }
            else
            {
                writer.Write($"var {Variable.Usage} = new {typeof(List<>).Namespace}.List<{ElementType.FullNameInCode()}>{{{elements}}};");
            }
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return Elements;
        }
    }
}
