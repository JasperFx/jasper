using System;
using System.Data.SqlClient;
using Jasper.SqlServer.Util;
using Oakton;

namespace RunLoadTests
{
    [Description("Delete all the table storage data in the target Sql Server database", Name = "clear-sql")]
    public class ClearSqlCommand : OaktonCommand<SqlInput>
    {
        public override bool Execute(SqlInput input)
        {
            using (var conn = new SqlConnection(input.ConnectionFlag))
            {
                conn.Open();

                deleteTable(conn, "receiver.jasper_outgoing_envelopes");
                deleteTable(conn, "receiver.jasper_incoming_envelopes");
                deleteTable(conn, "receiver.jasper_dead_letters");
                deleteTable(conn, "receiver.sent_track");
                deleteTable(conn, "receiver.received_track");

                deleteTable(conn, "sender.jasper_outgoing_envelopes");
                deleteTable(conn, "sender.jasper_incoming_envelopes");
                deleteTable(conn, "sender.jasper_dead_letters");
                deleteTable(conn, "sender.sent_track");
                deleteTable(conn, "sender.received_track");
            }

            ConsoleWriter.Write(ConsoleColor.Green, "Done!");

            return true;
        }

        private void deleteTable(SqlConnection conn, string tableName)
        {
            try
            {
                Console.WriteLine("Deleting data from " + tableName);
                conn.CreateCommand("delete from " + tableName).ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
