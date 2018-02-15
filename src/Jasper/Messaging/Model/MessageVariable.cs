using System;
using BlueMilk.Codegen.Variables;

namespace Jasper.Messaging.Model
{
    public class MessageVariable : Variable
    {
        public MessageVariable(Variable envelope, Type messageType) : base(messageType, DefaultArgName(messageType))
        {
            Creator = new MessageFrame(this, envelope);
        }
    }
}
