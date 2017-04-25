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

        public CaptureCascadingMessages(Variable messages, int position) : base(false)
        {
            messages.OverrideName("outgoing" + position);
            _messages = messages;
        }

        public override void GenerateCode(IGeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"{_context.Usage}.{nameof(IInvocationContext.EnqueueCascading)}({_messages.Usage});");
            Next?.GenerateCode(method, writer);
        }

        protected override IEnumerable<Variable> resolveVariables(IGeneratedMethod chain)
        {
            _context = chain.FindVariable(typeof(IInvocationContext));

            yield return _messages;
            yield return _context;
        }
    }
}
