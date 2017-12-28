using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Internals.Compilation;

namespace Jasper.Internals.Codegen
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


        public MethodCall(Type handlerType, MethodInfo method) : base(method.IsAsync())
        {
            HandlerType = handlerType;
            Method = method;

            if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
            {
                var variableType = method.ReturnType.CanBeCastTo<Task>()
                    ? method.ReturnType.GetTypeInfo().GetGenericArguments().First()
                    : method.ReturnType;

                var name = variableType.IsSimple() || variableType == typeof(object) || variableType == typeof(object[])
                    ? "result_of_" + method.Name
                    : Variable.DefaultArgName(variableType);

                ReturnVariable = new Variable(variableType, name, this);
            }

            Variables = new Variable[method.GetParameters().Length];
        }

        /// <summary>
        /// Call a method on the current object
        /// </summary>
        public bool IsLocal { get; set; }

        public Variable Target { get; set; }


        private Variable findVariable(ParameterInfo param, IMethodVariables chain)
        {
            var type = param.ParameterType;

            if (Aliases.ContainsKey(type))
            {
                var actualType = Aliases[type];
                var inner = chain.FindVariable(actualType);
                return new CastVariable(inner, type);
            }

            return chain.TryFindVariableByName(type, param.Name, out var variable) ? variable : chain.FindVariable(type);
        }

        public Variable[] Variables { get; }

        public bool TrySetParameter(Variable variable)
        {
            var parameters = Method.GetParameters().Select(x => x.ParameterType).ToArray();
            if (parameters.Count(x => variable.VariableType.CanBeCastTo(x)) == 1)
            {
                var index = Array.IndexOf(parameters, variable.VariableType);
                Variables[index] = variable;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySetParameter(string parameterName, Variable variable)
        {
            var parameters = Method.GetParameters().ToArray();
            var matching = parameters.FirstOrDefault(x =>
                variable.VariableType.CanBeCastTo(x.ParameterType) && x.Name == parameterName);

            if (matching == null) return false;

            var index = Array.IndexOf(parameters, matching);
            Variables[index] = variable;

            return true;
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            var parameters = Method.GetParameters().ToArray();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (Variables[i] != null)
                {
                    continue;
                }

                var param = parameters[i];
                Variables[i] = findVariable(param, chain);
            }

            foreach (var variable in Variables)
            {
                yield return variable;
            }

            if (Method.IsStatic || IsLocal) yield break;

            if (Target == null)
            {
                Target = chain.FindVariable(HandlerType);
            }

            yield return Target;
        }




        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var invokeMethod = invocationCode();

            var returnValue = "";

            if (IsAsync)
            {
                returnValue = method.AsyncMode == AsyncMode.ReturnFromLastNode ? "return " : "await ";
            }

            var isDisposable = false;
            if (ReturnVariable != null)
            {
                returnValue = $"var {ReturnVariable.Usage} = {returnValue}";
                isDisposable = ReturnVariable.VariableType.CanBeCastTo<IDisposable>();
            }

            if (isDisposable)
            {
                writer.UsingBlock($"{returnValue}{invokeMethod}", w => Next?.GenerateCode(method, writer));
            }
            else
            {
                writer.Write($"{returnValue}{invokeMethod};");

                Next?.GenerateCode(method, writer);
            }


        }

        private string invocationCode()
        {
            var methodName = Method.Name;
            if (Method.IsGenericMethod)
            {
                methodName += $"<{Method.GetGenericArguments().Select(x => x.FullName).Join(", ")}>";
            }

            var callingCode = $"{methodName}({Variables.Select(x => x.Usage).Join(", ")})";
            var target = determineTarget();
            var invokeMethod = $"{target}{callingCode}";
            return invokeMethod;
        }

        /// <summary>
        /// Code to invoke the method without any assignment to a variable
        /// </summary>
        /// <returns></returns>
        public string InvocationCode()
        {
            return IsAsync ? "await " + invocationCode() : invocationCode();
        }

        /// <summary>
        /// Code to invoke the method and set a variable to the returned value
        /// </summary>
        /// <returns></returns>
        public string AssignmentCode()
        {
            if (ReturnVariable == null)
            {
                throw new InvalidOperationException($"Method {this} does not have a return value");
            }

            return IsAsync
                ? $"var {ReturnVariable.Usage} = await {InvocationCode()}"
                : $"var {ReturnVariable.Usage} = {InvocationCode()}";

        }

        private string determineTarget()
        {
            if (IsLocal) return string.Empty;

            var target = Method.IsStatic
                ? HandlerType.FullNameInCode()
                : Target.Usage;

            return target + ".";
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
