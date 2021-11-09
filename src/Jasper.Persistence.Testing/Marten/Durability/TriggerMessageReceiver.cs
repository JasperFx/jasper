using System.Threading.Tasks;
using Jasper.Attributes;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Durability
{
    public class TriggerMessageReceiver
    {
        [Transactional]
        public Task Handle(TriggerMessage message, IDocumentSession session, IExecutionContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return context.RespondToSender(response);
        }
    }
}