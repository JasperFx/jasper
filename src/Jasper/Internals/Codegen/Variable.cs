using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Internals.Util;

namespace Jasper.Internals.Codegen
{
    public class Variable
    {
        public static Variable[] VariablesForProperties<T>(string rootArgName)
        {
            return typeof(T).GetTypeInfo().GetProperties().Where(x => x.CanRead)
                .Select(x => new Variable(x.PropertyType, $"{rootArgName}.{x.Name}"))
                .ToArray();
        }

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
            get => _frame;
            protected set
            {
                _frame = value;
                Creator?.creates.Fill(this);
            }
        }
        public Type VariableType { get; }
        public string Usage { get; protected set; }

        public bool CanBeReused { get; protected set; } = true;

        /// <summary>
        /// On rare occasions you may need to override the variable name
        /// </summary>
        /// <param name="variableName"></param>
        public void OverrideName(string variableName)
        {
            Usage = variableName;
        }

        public IList<Variable> Dependencies { get; } = new List<Variable>();

        public Variable(Type variableType) : this(variableType, DefaultArgName(variableType))
        {

        }

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

        public Variable(Type variableType, Frame creator) : this(variableType, DefaultArgName(variableType), creator)
        {

        }

        public override string ToString()
        {
            return $"{nameof(VariableType)}: {VariableType}, {nameof(Usage)}: {Usage}";
        }

        protected bool Equals(Variable other)
        {
            return VariableType == other.VariableType && string.Equals(Usage, other.Usage);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Variable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((VariableType != null ? VariableType.GetHashCode() : 0) * 397) ^ (Usage != null ? Usage.GetHashCode() : 0);
            }
        }
    }
}
