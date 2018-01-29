using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using BlueMilk.Compilation;
using Jasper.Bus.Runtime;

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
            writer.Write($"var {_message.Usage} = ({_message.VariableType.FullNameInCode()}){_envelope.Usage}.{nameof(Envelope.Message)};");
            Next?.GenerateCode(method, writer);
        }
    }
}
