using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Internal;
using Jasper.Util;

namespace Jasper.Codegen
{
    public abstract class HandlerGeneration<T> : IHandlerGeneration
    {
        public GenerationConfig Config { get; }
        public IList<Frame> Frames { get; }
        private readonly IVariableSource _specific;
        private readonly Dictionary<Type, Variable> _variables = new Dictionary<Type, Variable>();

        protected HandlerGeneration(string className, string inputName, IVariableSource specific, GenerationConfig config, IList<Frame> frames)
        {
            if (!frames.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(frames), "Cannot be an empty list");
            }

            var handlerType = typeof(T).FindInterfaceThatCloses(typeof(IHandler<>));
            if (handlerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(T), $"Type {typeof(T).FullName} does not implement IHandler<T>");
            }

            ClassName = className;
            Config = config;
            Frames = frames;
            _specific = specific;

            var inputType = handlerType.GetGenericArguments().Single();

            InputVariable = new Variable(inputType, inputName);

            var compiled = compileFrames(frames);

            if (compiled.All(x => !x.IsAsync))
            {
                AsyncMode = AsyncMode.ReturnCompletedTask;
            }
            else if (compiled.Count(x => x.IsAsync) == 1 && compiled.Last().IsAsync && compiled.Last().CanReturnTask())
            {
                AsyncMode = AsyncMode.ReturnFromLastNode;
            }

            Top = chainFrames(compiled);
        }

        private Frame[] compileFrames(IList<Frame> frames)
        {
            // Step 1, resolve all the necessary variables
            foreach (var frame in frames)
            {
                frame.ResolveVariables(this);
            }

            // Step 2, calculate dependencies
            foreach (var frame in frames)
            {
                frame.DetermineDependencies(this);
            }

            // Step 3, gather any missing frames and
            // add to the beginning of the list
            frames
                .SelectMany(x => x.Dependencies)
                .Distinct()
                .Where(x => !frames.Contains(x))
                .Each(x => frames.Insert(0, x));

            // Step 4, topological sort in dependency order
            return frames.TopologicalSort(x => x.Dependencies, true).ToArray();
        }

        private Frame chainFrames(Frame[] frames)
        {
            // Step 5, put into a chain.
            for (int i = 1; i < frames.Length; i++)
            {
                frames[i - 1].Next = frames[i];
            }

            return frames[0];
        }

        public Variable InputVariable { get; }
        public Frame Top { get; }

        public string ClassName { get; }
        public Type BaseType => typeof(T);

        public AsyncMode AsyncMode { get; } = AsyncMode.AsyncTask;

        public Variable FindVariable(Type type)
        {
            if (_variables.ContainsKey(type))
            {
                return _variables[type];
            }

            var variable = findVariable(type);
            _variables.Add(type, variable);

            return variable;
        }

        private Variable findVariable(Type type)
        {
            if (type == InputVariable.VariableType) return InputVariable;

            if (_specific.Matches(type))
            {
                return _specific.Create(type);
            }

            var created = Frames.SelectMany(x => x.Creates).FirstOrDefault(x => x.VariableType == type);
            if (created != null)
            {
                return created;
            }


            var source = Config.Sources.FirstOrDefault(x => x.Matches(type));
            if (source == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type),
                    $"Jasper doesn't know how to build a variable of type '{type.FullName}'");
            }

            return source.Create(type);
        }

        public InjectedField[] Fields => _variables.Values.OfType<InjectedField>().ToArray();
    }
}