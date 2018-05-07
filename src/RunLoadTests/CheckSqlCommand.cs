using System;
using System.Data.SqlClient;
using Jasper.SqlServer.Util;
using Oakton;

namespace RunLoadTests
{
    [Description("Checks the message sent and received counts", Name = "check-sql")]
    public class CheckSqlCommand : OaktonCommand<SqlInput>
    {
        public override bool Execute(SqlInput input)
        {
            using (var conn = new SqlConnection(input.ConnectionFlag))
            {
                conn.Open();

                var sentFromReceiver = (int)conn.CreateCommand("select count(*) from receiver.sent_track").ExecuteScalar();
                var receivedAtReceiver = (int)conn.CreateCommand("select count(*) from receiver.received_track").ExecuteScalar();

                var sentFromSender = (int)conn.CreateCommand("select count(*) from sender.sent_track").ExecuteScalar();
                var receivedAtSender = (int)conn.CreateCommand("select count(*) from sender.received_track").ExecuteScalar();


                if (sentFromSender == receivedAtReceiver)
                {
                    ConsoleWriter.Write(ConsoleColor.Green, "All messages successfully received from sender to receiver");
                }
                else
                {
                    ConsoleWriter.Write(ConsoleColor.Yellow, $"{sentFromSender} messages sent from Sender");
                    ConsoleWriter.Write(ConsoleColor.Yellow, $"{receivedAtReceiver} messages received at Receiver");
                }

                if (sentFromReceiver == receivedAtSender)
                {
                    ConsoleWriter.Write(ConsoleColor.Green, "All responses successfully received from receiver to sender");
                }
                else
                {
                    ConsoleWriter.Write(ConsoleColor.Yellow, $"{sentFromReceiver} responses sent from Receiver");
                    ConsoleWriter.Write(ConsoleColor.Yellow, $"{receivedAtSender} responses received at Sender");
                }
            }

            return true;
        }
    }
}
