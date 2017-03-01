using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;

namespace Jasper.Codegen
{
    public class Variable
    {
        public static Variable For<T>()
        {
            return new Variable(typeof(T), DefaultArgName(typeof(T)));
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

        private Frame _frame;

        public Frame Creator
        {
            get
            {
                return _frame;

            }
            protected set
            {
                _frame = value;
                Creator?.creates.Fill(this);
            }
        }
        public Type VariableType { get; }
        public string Usage { get; private set; }

        /// <summary>
        /// On rare occasions you may need to override the variable name
        /// </summary>
        /// <param name="variableName"></param>
        public void OverrideName(string variableName)
        {
            Usage = variableName;
        }

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

        public override string ToString()
        {
            return $"{nameof(VariableType)}: {VariableType}, {nameof(Usage)}: {Usage}";
        }
    }
}