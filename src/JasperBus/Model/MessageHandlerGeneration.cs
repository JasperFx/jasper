using System.Collections.Generic;
using Jasper.Codegen;

namespace JasperBus.Model
{
    public class MessageHandlerGeneration : HandlerGeneration<MessageHandler>
    {
        public MessageHandlerGeneration(string className, GenerationConfig config, IList<Frame> frames) : base(className, "context", MessageHandlerVariableSource.Instance, config, frames)
        {
        }
    }
}