using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Jasper.Internals.Util;

namespace Jasper.Internals.IoC
{
    public class ListAssignmentFrame : Frame
    {
        public ListAssignmentFrame(Type serviceType, Variable[] elements) : base(false)
        {
            ElementType = EnumerableStep.DetermineElementType(serviceType);
            Variable = new Variable(serviceType, Variable.DefaultArgName(ElementType) + "List", this);

            Elements = elements;
        }

        public Type ElementType { get; }

        public Variable[] Elements { get; }

        public Variable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var elements = Elements.Select(x => x.Usage).Join(", ");
            writer.Write($"var {Variable.Usage} = new {typeof(List<>).Namespace}.List<{ElementType.FullNameInCode()}>{{{elements}}};");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return Elements;
        }
    }
}
