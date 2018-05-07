using System;
using Jasper.Messaging.Runtime;
using Oakton;
using TestMessages;

namespace RunLoadTests
{
    [Description("Build the postgres schema objects", Name = "build-postgres")]
    public class BuildPostgresCommand : OaktonCommand<PostgresInput>
    {
        public override bool Execute(PostgresInput input)
        {
            Console.WriteLine("Building objects in the receiver schema");
            using (var store = input.StoreForSchema("receiver"))
            {
                store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
                store.Tenancy.Default.EnsureStorageExists(typeof(SentTrack));
                store.Tenancy.Default.EnsureStorageExists(typeof(ReceivedTrack));
            }

            Console.WriteLine("Building objects in the sender schema");
            using (var store = input.StoreForSchema("sender"))
            {
                store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
                store.Tenancy.Default.EnsureStorageExists(typeof(SentTrack));
                store.Tenancy.Default.EnsureStorageExists(typeof(ReceivedTrack));
            }

            ConsoleWriter.Write(ConsoleColor.Green, "Success!");

            return true;
        }
    }
}
