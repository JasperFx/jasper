using System;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Model
{
    public class MessageHandlerVariableSource : IVariableSource
    {

        public Type MessageType { get; }



        public MessageHandlerVariableSource(Type messageType)
        {
            MessageType = messageType;

            Envelope = new Variable(typeof(Envelope), $"{HandlerGraph.Context}.{nameof(IInvocationContext.Envelope)}");
            Message = new MessageVariable(Envelope, messageType);

        }

        public MessageVariable Message { get; }
        public Variable Envelope { get; }

        public bool Matches(Type type)
        {
            return type == typeof(Envelope) || type == MessageType;
        }

        public Variable Create(Type type)
        {
            if (type == MessageType)
            {
                return Message;
            }

            if (type == typeof(Envelope))
            {
                return Envelope;
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
