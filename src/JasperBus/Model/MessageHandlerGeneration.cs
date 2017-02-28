using System;
using System.Collections.Generic;
using Jasper.Codegen;

namespace JasperBus.Model
{
    public class MessageHandlerGeneration : HandlerGeneration<MessageHandler>
    {
        public MessageHandlerGeneration(string className, Type messageType, GenerationConfig config, IList<Frame> frames)
            : base(className, MessageHandlerGraph.Context, new MessageHandlerVariableSource(messageType), config, frames)
        {
        }
    }
}