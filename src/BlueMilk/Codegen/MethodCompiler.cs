using System;
using System.Collections.Generic;
using BlueMilk.IoC;

namespace BlueMilk.Codegen
{
    public class MethodCompiler
    {
        private readonly GeneratedMethod _method;
        private readonly ServiceGraph _services;
        private readonly GeneratedClass _class;
        private readonly Dictionary<Type, Variable> _variables = new Dictionary<Type, Variable>();

        public MethodCompiler(GeneratedMethod method, ServiceGraph services, GeneratedClass @class)
        {
            _method = method;
            _services = services;
            _class = @class;
        }

        public void Compile()
        {

        }
    }
}
