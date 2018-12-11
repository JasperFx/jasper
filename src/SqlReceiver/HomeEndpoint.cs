using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Jasper;
using Jasper.Persistence;
using Jasper.Persistence.SqlServer.Util;

namespace SqlReceiver
{
    public static class HomeEndpoint
    {
        public static string Index(JasperRuntime runtime)
        {
            var writer = new StringWriter();
            runtime.Describe(writer);

            return writer.ToString();
        }

        [Transactional]
        public static Task post_clear(SqlTransaction tx)
        {
            return tx.Connection.CreateCommand("delete from receiver.sent_track;delete from receiver.received_track")
                .ExecuteNonQueryAsync();
        }
    }
}
