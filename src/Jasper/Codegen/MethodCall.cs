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
    public class MethodCall : Frame
    {
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



        protected override IEnumerable<Variable> resolveVariables(IHandlerGeneration chain)
        {
            _variables = Method.GetParameters()
                .Select(param => chain.FindVariable(param.ParameterType))
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

        public override void GenerateCode(IHandlerGeneration generation, ISourceWriter writer)
        {
            var callingCode = $"{Method.Name}({_variables.Select(x => x.Usage).Join(", ")})";
            var target = Method.IsStatic
                ? HandlerType.FullName
                : _target.Usage;

            var returnValue = "";
            var suffix = "";

            if (IsAsync)
            {
                if (generation.AsyncMode == AsyncMode.ReturnFromLastNode)
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

            Next?.GenerateCode(generation, writer);
        }


        public override bool CanReturnTask()
        {
            return IsAsync;
        }
    }
}