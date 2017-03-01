using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class CastVariable : Variable
    {
        public CastVariable(Variable parent, Type specificType) : base(specificType, $"(({specificType.FullName}){parent.Usage})")
        {
            Dependencies.Add(parent);
        }
    }

    public class MethodCall : Frame
    {
        public Dictionary<Type, Type> Aliases { get; } = new Dictionary<Type, Type>();

        public Type HandlerType { get; }
        public MethodInfo Method { get; }

        public static MethodCall For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new MethodCall(typeof(T), method);
        }

        private Variable _target;
        private Variable[] _variables;


        // What's it got to know?
        // whether it returns a variable

        public MethodCall(Type handlerType, MethodInfo method) : base(method.IsAsync())
        {
            HandlerType = handlerType;
            Method = method;
        }

        private Type typeForParameter(ParameterInfo param)
        {
            var type = param.ParameterType;
            if (Aliases.ContainsKey(type)) return Aliases[type];

            return type;
        }

        private Variable findVariable(ParameterInfo param, IGenerationModel chain)
        {
            var type = param.ParameterType;

            if (Aliases.ContainsKey(type))
            {
                var actualType = Aliases[type];
                var inner = chain.FindVariable(actualType);
                return new CastVariable(inner, type);
            }

            return chain.FindVariable(type);
        }

        protected override IEnumerable<Variable> resolveVariables(IGenerationModel chain)
        {
            _variables = Method.GetParameters()
                .Select(param => findVariable(param, chain))
                .ToArray();

            foreach (var variable in _variables)
            {
                yield return variable;
            }

            if (!Method.IsStatic)
            {
                _target = chain.FindVariable(HandlerType);
                yield return _target;
            }
        }


        public override void GenerateCode(IGenerationModel generationModel, ISourceWriter writer)
        {
            var callingCode = $"{Method.Name}({_variables.Select(x => x.Usage).Join(", ")})";
            var target = Method.IsStatic
                ? HandlerType.FullName
                : _target.Usage;

            var returnValue = "";
            var suffix = "";

            if (IsAsync)
            {
                if (generationModel.AsyncMode == AsyncMode.ReturnFromLastNode)
                {
                    returnValue = "return ";
                }
                else
                {
                    // TODO -- going to need to see if it returns something
                    returnValue = "await ";
                }
            }

            // TODO -- will need to see if it's IDisposable too
            // TODO -- will have to deal with frames that declare a variable

            writer.Write($"{returnValue}{target}.{callingCode}{suffix};");

            Next?.GenerateCode(generationModel, writer);
        }


        public override bool CanReturnTask()
        {
            return IsAsync;
        }
    }
}