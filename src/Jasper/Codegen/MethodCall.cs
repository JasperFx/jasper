using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class MethodCall : Frame
    {
        public Dictionary<Type, Type> Aliases { get; } = new Dictionary<Type, Type>();

        public Type HandlerType { get; }
        public MethodInfo Method { get; }
        public Variable ReturnVariable { get; private set; }

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

            if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
            {
                var variableType = method.ReturnType.CanBeCastTo<Task>()
                    ? method.ReturnType.GetGenericArguments().First()
                    : method.ReturnType;

                var name = variableType.IsSimple() || variableType == typeof(object) || variableType == typeof(object[])
                    ? "result_of_" + method.Name
                    : Variable.DefaultArgName(variableType);

                ReturnVariable = new Variable(variableType, name, this);
            }
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
                ? HandlerType.NameInCode()
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
                    returnValue = "await ";
                    suffix = ".ConfigureAwait(false)";
                }
            }

            if (ReturnVariable != null)
            {
                returnValue = $"var {ReturnVariable.Usage} = {returnValue}";
            }

            // TODO -- will need to see if it's IDisposable too

            writer.Write($"{returnValue}{target}.{callingCode}{suffix};");

            Next?.GenerateCode(generationModel, writer);
        }


        public override bool CanReturnTask()
        {
            return IsAsync;
        }

        public override string ToString()
        {
            return $"{nameof(HandlerType)}: {HandlerType}, {nameof(Method)}: {Method}";
        }
    }
}