using System;
using Baseline;

namespace Jasper.Core.Codegen
{
    public class Arg
    {
        public static Arg For<T>(string name = null)
        {
            if (name.IsEmpty())
            {
                var fullname = typeof(T).Name;
                name = fullname.Substring(0, 1) + fullname.Substring(1, fullname.Length - 1);
            }

            return new Arg(name, typeof(T));
        }

        public Type Type { get; }
        public string TypeName { get; set; }
        public string Name { get;}

        public Arg(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}