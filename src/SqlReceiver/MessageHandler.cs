using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.SqlServer;
using TestMessages;

namespace SqlReceiver
{
    public static class MessageHandler
    {
        [SqlTransaction]
        public static Task Handle(Target target, SqlTransaction tx)
        {
            return tx.StoreReceived(target.Id, "Target");
        }

        [SqlTransaction]
        public static Task Handle(UserCreated created, SqlTransaction tx)
        {
            return tx.StoreReceived(created.Id, "UserCreated");
        }

        [SqlTransaction]
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
