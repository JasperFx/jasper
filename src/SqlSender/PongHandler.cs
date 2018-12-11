using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Persistence;
using TestMessages;

namespace SqlSender
{
    public class PongHandler
    {
        [Transactional]
        public Task Handle(PongMessage message, SqlTransaction tx)
        {
            return tx.StoreReceived(message.Id, "PongMessage");
        }
    }
}
