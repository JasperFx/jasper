using Jasper.Bus.Runtime;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;

namespace Jasper.Bus.Model
{
    public class MessageFrame : Frame
    {
        private readonly MessageVariable _message;
        private readonly Variable _envelope;

        public MessageFrame(MessageVariable message, Variable envelope) : base(false)
        {
            _message = message;
            _envelope = envelope;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {_message.Usage} = ({_message.VariableType.NameInCode()}){_envelope.Usage}.{nameof(Envelope.Message)};");
            Next?.GenerateCode(method, writer);
        }
    }
}
