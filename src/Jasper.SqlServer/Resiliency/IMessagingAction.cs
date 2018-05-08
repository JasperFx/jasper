using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;

namespace Jasper.SqlServer.Resiliency
{
    public interface IMessagingAction
    {
        Task Execute(SqlConnection conn, ISchedulingAgent agent);
    }
}
