using System;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Durability;
using Microsoft.Extensions.DependencyInjection;
using Oakton;

namespace Jasper.Persistence
{

    public enum StorageAction
    {
        clear,
        counts,
        rebuild,
        script,
        release

    }

    public class StorageInput : NetCoreInput
    {
        [Description("Choose the action")] public StorageAction Action { get; set; } = StorageAction.counts;

        [Description("Optional, specify the file where the schema script would be written")]
        public string FileFlag { get; set; } = "storage.sql";
    }

    [Description("Administer the envelope storage")]
    public class StorageCommand : OaktonAsyncCommand<StorageInput>
    {
        public StorageCommand()
        {
            Usage("Administer the envelope storage").Arguments(x => x.Action);
        }

        public override async Task<bool> Execute(StorageInput input)
        {
            using (var host = input.BuildHost())
            {
                var persistor = host.Services.GetRequiredService<IEnvelopePersistence>();

                persistor.Describe(Console.Out);

                switch (input.Action)
                {
                    case (StorageAction.counts):

                        await persistor.Admin.RebuildSchemaObjects();

                        var counts = await persistor.Admin.GetPersistedCounts();
                        Console.WriteLine("Persisted Enveloper Counts");
                        Console.WriteLine($"Incoming    {counts.Incoming.ToString().PadLeft(5)}");
                        Console.WriteLine($"Outgoing    {counts.Outgoing.ToString().PadLeft(5)}");
                        Console.WriteLine($"Scheduled   {counts.Scheduled.ToString().PadLeft(5)}");

                        break;

                    case (StorageAction.clear):
                        await persistor.Admin.ClearAllPersistedEnvelopes();
                        ConsoleWriter.Write(ConsoleColor.Green, "Successfully deleted all persisted envelopes");
                        break;

                    case (StorageAction.rebuild):
                        await persistor.Admin.RebuildSchemaObjects();
                        ConsoleWriter.Write(ConsoleColor.Green, "Successfully rebuilt the envelope storage");
                        break;

                    case (StorageAction.script):
                        Console.WriteLine("Exporting script to " + input.FileFlag.ToFullPath());
                        await File.WriteAllTextAsync(input.FileFlag, persistor.Admin.CreateSql());

                        break;

                    case StorageAction.release:
                        await persistor.Admin.RebuildSchemaObjects();
                        Console.WriteLine("Releasing all ownership of persisted envelopes");
                        await persistor.Admin.ReleaseAllOwnership();

                        break;
                }
            }

            return true;
        }
    }

}
