using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;

namespace Jasper.Codegen
{
    public class Variable
    {
        public static string DefaultArgName(Type argType)
        {
            var parts = argType.Name.SplitPascalCase().Split(' ');
            if (argType.GetTypeInfo().IsInterface && parts.First() == "I")
            {
                parts = parts.Skip(1).ToArray();
            }

            return parts.First().ToLower() + parts.Skip(1).Join("");
        }

        public Frame Creator { get; protected set; }
        public Type VariableType { get; }
        public string Usage { get; }

        public IList<Variable> Dependencies { get; } = new List<Variable>();

        public Variable(Type variableType, string usage)
        {
            VariableType = variableType;
            Usage = usage;
        }

        public Variable(Type variableType, string usage, Frame creator) : this(variableType, usage)
        {
            Creator = creator;

            Creator.creates.Fill(this);
        }
    }
}