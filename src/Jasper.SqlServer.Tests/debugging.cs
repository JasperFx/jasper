using System.Data.SqlClient;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public class debugging
    {
        [Fact]
        public void open_a_connection()
        {
            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                conn.Open();
            }

        }
    }
}
