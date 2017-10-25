using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Jasper.Internals.Util;

namespace Jasper.Internals.IoC
{
    public class ConstructorFrame : SyncFrame
    {
        private readonly Variable[] _arguments;
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public Variable Variable { get; }

        public ConstructorFrame(Type serviceType, Type implementationType, string variableName, Variable[] arguments)
        {
            _arguments = arguments;
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Variable = new Variable(serviceType, variableName, this);
        }

        public int Number { get; set; }

        public bool IsDisposable => ImplementationType.CanBeCastTo<IDisposable>();

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var arguments = _arguments.Select(x => x.Usage).Join(", ");
            var implementationTypeName = ImplementationType.FullName.Replace("+", ".");

            var declaration = $"var {Variable.Usage} = new {implementationTypeName}({arguments})";

            if (IsDisposable)
            {
                if (Next is ConstructorFrame && Next.As<ConstructorFrame>().IsDisposable)
                {
                    writer.Write($"using ({declaration})");
                    Next?.GenerateCode(method, writer);
                }
                else
                {
                    writer.UsingBlock(declaration, w => Next?.GenerateCode(method, w));
                }
            }
            else
            {
                writer.Write(declaration + ";");
                Next?.GenerateCode(method, writer);
            }




        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return _arguments;
        }
    }
}
