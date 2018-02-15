using Jasper.Messaging.Model;

namespace Jasper.Diagnostics.Messages
{
    public class ChainModel
    {
        public MessageTypeModel MessageType { get; set; }
        public string GeneratedTypeName { get; set; }
        public string Description { get; set; }
        public string SourceCode { get; set; }

        public static ChainModel For(HandlerChain chain)
        {
            return new ChainModel
            {
                MessageType = new MessageTypeModel(chain.MessageType),
                GeneratedTypeName = chain.TypeName,
                Description = chain.ToString(),
                SourceCode = chain.SourceCode
            };
        }
    }
}