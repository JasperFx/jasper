using System.Threading.Tasks;
using Jasper.Attributes;

namespace Jasper.Persistence.Testing.SqlServer.Durability
{
    public class TriggerMessageReceiver
    {
        [Transactional]
        public Task Handle(TriggerMessage message, IExecutionContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return context.RespondToSender(response);
        }
    }
}