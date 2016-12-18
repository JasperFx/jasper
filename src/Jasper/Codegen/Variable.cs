using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Codegen.Compilation;
using Jasper.Util;

namespace Jasper.Codegen
{
    public abstract class Variable
    {
        public static Variable[] GatherAllDependencies(IEnumerable<Variable> variables)
        {
            var list = new List<Variable>(variables);

            foreach (var variable in variables)
            {
                variable.gather(list);
            }

            return list.TopologicalSort(v => v.Dependencies).ToArray();
        }

        private void gather(List<Variable> list)
        {
            foreach (var dependency in Dependencies)
            {
                if (list.Contains(dependency)) continue;

                list.Add(dependency);
                dependency.gather(list);
            }
        }

        public static string DefaultArgName(Type argType)
        {
            var parts = argType.Name.SplitPascalCase().Split(' ');
            if (argType.GetTypeInfo().IsInterface && parts.First() == "I")
            {
                parts = parts.Skip(1).ToArray();
            }

            return parts.First().ToLower() + parts.Skip(1).Join("");
        }

        public Variable(Type argType, VariableCreation creation = VariableCreation.Injected)
            : this(argType, DefaultArgName(argType), creation)
        {
        }

        public Variable(Type argType, string name, VariableCreation creation)
        {
            Name = name;
            Creation = creation;
            VariableType = argType;
        }

        public string Name { get; }
        public VariableCreation Creation { get; }
        public Type VariableType { get; }

        public virtual IEnumerable<Variable> Dependencies
        {
            get
            {
                yield break;
            }
        }


        public void GenerateCreationCode(HandlerGeneration generation, Frame frame, ISourceWriter writer)
        {
            if (ReferenceEquals(this, frame.Instantiates.Last()))
            {
                generateCreation(writer, w => frame.generateCode(generation, writer));
                return;
            }

            var index = Array.IndexOf(frame.Instantiates, this);
            var next = frame.Instantiates[index + 1];
            generateCreation(writer, w => next.GenerateCreationCode(generation, frame, writer));
        }

        protected virtual void generateCreation(ISourceWriter writer, Action<ISourceWriter> continuation)
        {
            throw new NotSupportedException("This feature has to be implemented for this type of Variable");
        }
    }
}