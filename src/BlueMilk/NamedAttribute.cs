using System;

namespace BlueMilk
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter)]
    public class NamedAttribute : Attribute
    {
        public string Name { get; }

        public NamedAttribute(string name)
        {
            Name = name;
        }
    }
}