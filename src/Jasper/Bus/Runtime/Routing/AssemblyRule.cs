using System;
using System.Reflection;

namespace Jasper.Bus.Runtime.Routing
{
    public class AssemblyRule : IRoutingRule
    {
        public AssemblyRule(Assembly assembly)
        {
            Assembly = assembly;
        }


        public Assembly Assembly { get; }

        public bool Matches(Type type)
        {
            return Assembly.Equals(type.GetTypeInfo().Assembly);
        }

        public string Describe()
        {
            return "Messages in Assembly " + Assembly.GetName().Name;
        }

        public static AssemblyRule For<T>()
        {
            return new AssemblyRule(typeof(T).GetTypeInfo().Assembly);
        }

        protected bool Equals(AssemblyRule other)
        {
            return Equals(Assembly, other.Assembly);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyRule) obj);
        }

        public override int GetHashCode()
        {
            return (Assembly != null ? Assembly.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"Contained in assembly {Assembly.GetName().Name}";
        }
    }

}