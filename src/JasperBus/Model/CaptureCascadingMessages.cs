using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using JasperBus.Runtime.Invocation;

namespace JasperBus.Model
{
    public class CaptureCascadingMessages : Frame
    {
        private readonly Variable _messages;
        private Variable _context;

        public CaptureCascadingMessages(Variable messages) : base(false)
        {
            _messages = messages;
        }

        public override void GenerateCode(IGenerationModel generationModel, ISourceWriter writer)
        {
            writer.Write($"{_context.Usage}.{nameof(IInvocationContext.EnqueueCascading)}({_messages.Usage});");
            Next?.GenerateCode(generationModel, writer);
        }

        protected override IEnumerable<Variable> resolveVariables(IGenerationModel chain)
        {
            _context = chain.FindVariable(typeof(IInvocationContext));

            yield return _messages;
            yield return _context;
        }
    }
}