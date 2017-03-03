using System;
using System.Collections.Generic;
using Jasper.Codegen;

namespace JasperBus.Model
{
    public class MessageHandlerGenerationModel : GenerationModel<MessageHandler>
    {
        public MessageHandlerGenerationModel(string className, Type messageType, GenerationConfig config, IList<Frame> frames)
            : base(className, HandlerGraph.Context, new MessageHandlerVariableSource(messageType), config, frames)
        {
        }
    }
}