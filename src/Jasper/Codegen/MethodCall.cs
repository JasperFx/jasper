using System;
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
        public static MethodCall For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new MethodCall(typeof(T), method);
        }

        private readonly Type _handlerType;
        private readonly MethodInfo _method;
        private Variable[] _variables = new Variable[0];
        private Variable _target;


        // What's it got to know?
        // whether it returns a variable

        public MethodCall(Type handlerType, MethodInfo method) : base(method.IsAsync())
        {
            _handlerType = handlerType;
            _method = method;
        }

        public override void ResolveVariables(HandlerGeneration chain)
        {
            _variables = _method.GetParameters()
                .Select(param => chain.FindVariable(param.ParameterType))
                .ToArray();

            if (!_method.IsStatic)
            {
                _target = chain.FindVariable(_handlerType);
            }
        }

        public override void GenerateCode(HandlerGeneration generation, ISourceWriter writer)
        {
            var callingCode = $"{_method.Name}({_variables.Select(x => x.Name).Join(", ")})";
            var target = _method.IsStatic
                ? _handlerType.FullName
                : _target.Name;

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