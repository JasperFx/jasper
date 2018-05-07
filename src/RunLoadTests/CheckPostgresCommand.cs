using System;
using System.Linq;
using Oakton;
using TestMessages;

namespace RunLoadTests
{
    [Description("Check the message completion in the postgres database", Name = "check-postgres")]
    public class CheckPostgresCommand : OaktonCommand<PostgresInput>
    {
        public override bool Execute(PostgresInput input)
        {
            var sentFromSender = 0;
            var receivedAtSender = 0;


            using (var store = input.StoreForSchema("receiver"))
            {
                using (var session = store.QuerySession())
                {
                    sentFromSender = session.Query<SentTrack>().Count();
                    receivedAtSender = session.Query<ReceivedTrack>().Count();
                }
            }


            var sentFromReceiver = 0;
            var receivedAtReceiver = 0;

            using (var store = input.StoreForSchema("sender"))
            {
                using (var session = store.QuerySession())
                {
                    sentFromReceiver = session.Query<SentTrack>().Count();
                    receivedAtReceiver = session.Query<ReceivedTrack>().Count();
                }
            }

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


            return true;
        }
    }
}