using System;
using Oakton;

namespace RunLoadTests
{
    [Description("Clears the postgres table state", Name = "clear-postgres")]
    public class ClearPostgresCommand : OaktonCommand<PostgresInput>
    {
        public override bool Execute(PostgresInput input)
        {
            using (var store = input.StoreForSchema("receiver"))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            using (var store = input.StoreForSchema("sender"))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            ConsoleWriter.Write(ConsoleColor.Green, "Success!");

            return true;
        }
    }
}
