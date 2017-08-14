using System.Collections.Generic;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;

namespace Jasper.Bus.Model
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

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"{_context.Usage}.{nameof(IInvocationContext.EnqueueCascading)}({_messages.Usage});");
            Next?.GenerateCode(method, writer);
        }

        protected internal override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            _context = chain.FindVariable(typeof(IInvocationContext));

            yield return _messages;
            yield return _context;
        }
    }
}
