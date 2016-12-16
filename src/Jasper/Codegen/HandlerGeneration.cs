using System;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Codegen
{
    public enum AsyncMode
    {
        ReturnCompletedTask,
        AsyncTask,
        ReturnFromLastNode
    }

    public class HandlerGeneration : Variable
    {
        private readonly GenerationConfig _config;

        public HandlerGeneration(HandlerChain chain, GenerationConfig config, string inputArg)
            : base(chain.InputType, inputArg)
        {
            _config = config;

            if (chain.All(x => !x.IsAsync))
            {
                AsyncMode = AsyncMode.ReturnCompletedTask;
            }
            else if (chain.Count(x => x.IsAsync) == 1 && chain.Last().IsAsync && chain.Last().CanReturnTask())
            {
                AsyncMode = AsyncMode.ReturnFromLastNode;
            }
        }

        // TODO -- may need to track name as well
        public Variable FindVariable(Type type)
        {
            if (type == VariableType) return this;
            foreach (var configSource in _config.Sources)
            {
                Console.WriteLine(configSource);
            }
            var source = _config.Sources.FirstOrDefault(x => x.Matches(type));
            if (source == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type), $"Jasper doesn't know how to build a variable of type '{type.FullName}'");
            }

            return source.Create(type);
        }

        public AsyncMode AsyncMode { get; } = AsyncMode.AsyncTask;

        public readonly IList<InjectedField> Fields = new List<InjectedField>();

    }
}