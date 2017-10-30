using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Internals.Compilation;
using Jasper.Internals.Util;

namespace Jasper.Internals.Codegen
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
            var compiler = new MethodFrameArranger(this, @class);
            compiler.Arrange();
        }

        public string ToExitStatement()
        {
            if (AsyncMode == AsyncMode.AsyncTask) return "return;";

            return $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";
        }

        public AsyncMode AsyncMode { get; internal set; } = AsyncMode.AsyncTask;

        public Frame Top { get; internal set; }

        public InjectedField[] Fields { get; internal set; } = new InjectedField[0];

        public IList<Frame> Frames { get; }

        public IEnumerable<Argument> Arguments => _arguments;

        public Visibility Visibility { get; set; } = Visibility.Public;

        public readonly IList<Variable> DerivedVariables = new List<Variable>();

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();


    }
}

