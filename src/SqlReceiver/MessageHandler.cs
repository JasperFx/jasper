using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Persistence;
using Jasper.Persistence.SqlServer;
using TestMessages;

namespace SqlReceiver
{
    [Transaction]
    public static class MessageHandler
    {
        public static Task Handle(Target target, SqlTransaction tx)
        {
            return tx.StoreReceived(target.Id, "Target");
        }

        public static Task Handle(UserCreated created, SqlTransaction tx)
        {
            return tx.StoreReceived(created.Id, "UserCreated");
        }

        public static async Task<PongMessage> Handle(PingMessage message, SqlTransaction tx)
        {
            await tx.StoreReceived(message.Id, "PingMessage");
            await tx.StoreSent(message.Id, "PongMessage");

            return new PongMessage
            {
                Id = message.Id,
                Name = message.Name
            };
        }
    }
}
