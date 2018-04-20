using System.Data.SqlClient;
using Jasper.SqlServer.Tests;
using Xunit;

namespace Jasper.Marten.Tests.Setup
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
