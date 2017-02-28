using System;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using JasperBus.Runtime;

namespace JasperBus.Model
{
    public class MessageFrame : Frame
    {
        private readonly Type _messageType;
        private readonly Variable _envelope;

        public MessageFrame(Type messageType, Variable envelope) : base(false)
        {
            _messageType = messageType;
            _envelope = envelope;
        }

        public override void GenerateCode(IHandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"var messsage = ({_messageType.FullName}){_envelope.Usage}.{nameof(Envelope.Message)}");
            Next?.GenerateCode(generation, writer);
        }
    }
}