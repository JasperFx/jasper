using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen.Compilation;
using Jasper.Util;

namespace Jasper.Codegen
{
    public interface IGeneratedMethod
    {
        string MethodName { get; }
        bool Virtual { get; set; }
        AsyncMode AsyncMode { get; }
        IEnumerable<Argument> Arguments { get; }
    }

    public class GeneratedMethod : IGeneratedMethod
    {
        private readonly Argument[] _arguments;
        private readonly Dictionary<Type, Variable> _variables = new Dictionary<Type, Variable>();

        public string MethodName { get; }

        public GeneratedMethod(string methodName, Argument[] arguments, IList<Frame> frames)
        {
            if (!frames.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(frames), "Cannot be an empty list");
            }

            _arguments = arguments;
            MethodName = methodName;
            Frames = frames;
        }


        public bool Overrides { get; set; }
        public bool Virtual { get; set; }

        public void WriteMethod(ISourceWriter writer)
        {
            var returnValue = AsyncMode == AsyncMode.AsyncTask
                ? "async Task"
                : "Task";

            if (Overrides)
            {
                returnValue = "override " + returnValue;
            }

            var arguments = Arguments.Select(x => x.Declaration).Join(", ");

            writer.Write($"BLOCK:public {returnValue} {MethodName}({arguments})");


            Top.GenerateCode(this, writer);

            if (AsyncMode == AsyncMode.ReturnCompletedTask)
            {
                writer.Write("return Task.CompletedTask;");
            }

            writer.FinishBlock();


        }

        public void ArrangeFrames(GeneratedClass @class)
        {
            _class = @class;

            var compiled = compileFrames(Frames);

            if (compiled.All(x => !x.IsAsync))
            {
                AsyncMode = AsyncMode.ReturnCompletedTask;
            }
            else if (compiled.Count(x => x.IsAsync) == 1 && compiled.Last().IsAsync && compiled.Last().CanReturnTask())
            {
                AsyncMode = compiled.Any(x => x.Wraps) ? AsyncMode.AsyncTask : AsyncMode.ReturnFromLastNode;
            }

            Top = chainFrames(compiled);
        }

        public string ToExitStatement()
        {
            if (AsyncMode == AsyncMode.AsyncTask) return "return;";

            return $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";
        }

        public AsyncMode AsyncMode { get; private set; } = AsyncMode.AsyncTask;

        public Frame Top { get; private set; }

        private Frame[] compileFrames(IList<Frame> frames)
        {
            // Step 1, resolve all the necessary variables
            foreach (var frame in frames)
            {
                frame.ResolveVariables(this);
            }

            // Step 2, calculate dependencies
            var dependencies = new DependencyGatherer(frames);
            findInjectedFields(dependencies);

            // Step 3, gather any missing frames and
            // add to the beginning of the list
            dependencies.Dependencies.GetAll().SelectMany(x => x).Distinct()
                .Where(x => !frames.Contains(x))
                .Each(x => frames.Insert(0, x));

            // Step 4, topological sort in dependency order
            return frames.TopologicalSort(x => dependencies.Dependencies[x], true).ToArray();
        }

        public InjectedField[] Fields { get; protected set; } = new InjectedField[0];

        private void findInjectedFields(DependencyGatherer dependencies)
        {
            // Stupid. Can't believe I haven't fixed this in Baseline
            var list = new List<InjectedField>();
            dependencies.Variables.Each((key, _) =>
            {
                if (key is InjectedField)
                {
                    list.Add(key.As<InjectedField>());
                }
            });

            Fields = list.ToArray();
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

        public IList<Frame> Frames { get; }

        public IEnumerable<Argument> Arguments => _arguments;

        public Visibility Visibility { get; set; } = Visibility.Public;

        public readonly IList<Variable> DerivedVariables = new List<Variable>();
        private GeneratedClass _class;

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

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        private Variable findVariable(Type type)
        {
            var argument = Arguments.Concat(DerivedVariables).FirstOrDefault(x => x.VariableType == type);
            if (argument != null)
            {
                return argument;
            }

            var created = Frames.SelectMany(x => x.Creates).FirstOrDefault(x => x.VariableType == type);
            if (created != null)
            {
                return created;
            }

            var source = Sources.Concat(_class.Config.Sources).FirstOrDefault(x => x.Matches(type));
            if (source != null)
            {
                return source.Create(type);
            }


            throw new ArgumentOutOfRangeException(nameof(type),
                $"Jasper doesn't know how to build a variable of type '{type.FullName}'");


        }

        public Variable FindVariableByName(Type dependency, string name)
        {
            Variable variable;
            if (TryFindVariableByName(dependency, name, out variable)) return variable;

            throw new ArgumentOutOfRangeException(nameof(dependency), $"Cannot find a matching variable {dependency.FullName} {name}");
        }

        public bool TryFindVariableByName(Type dependency, string name, out Variable variable)
        {
            variable = null;

            var sourced = Sources.Where(x => x.Matches(dependency)).Select(x => x.Create(dependency));
            var created = Frames.SelectMany(x => x.Creates);

            var candidate = _variables.Values
                .Concat(Arguments)
                .Concat(DerivedVariables)
                .Concat(created)
                .Concat(sourced)
                .Where(x => x != null)
                .FirstOrDefault(x => x.VariableType == dependency && x.Usage == name);


            if (candidate != null)
            {
                variable = candidate;
                return true;
            }

            return false;
        }
    }
}

