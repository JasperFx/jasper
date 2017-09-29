using System;
using System.Collections.Generic;
using System.Linq;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using BlueMilk.Util;

namespace BlueMilk.IoC
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

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var arguments = _arguments.Select(x => x.Usage).Join(", ");
            var implementationTypeName = ImplementationType.FullName.Replace("+", ".");
            writer.Write($"var {Variable.Usage} = new {implementationTypeName}({arguments});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return _arguments;
        }
    }
}