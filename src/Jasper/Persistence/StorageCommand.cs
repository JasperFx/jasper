using System;
using System.IO;
using Baseline;
using Jasper.Messaging.Durability;
using Oakton;
using Oakton.AspNetCore;

namespace Jasper.Persistence
{

    public enum StorageAction
    {
        clear,
        counts,
        rebuild,
        script

    }

    public class StorageInput : NetCoreInput
    {
        [Description("Choose the action")] public StorageAction Action { get; set; } = StorageAction.counts;

        [Description("Optional, specify the file where the schema script would be written")]
        public string FileFlag { get; set; } = "storage.sql";
    }

    [Description("Administer the envelope storage")]
    public class StorageCommand : OaktonCommand<StorageInput>
    {
        public StorageCommand()
        {
            Usage("Administer the envelope storage").Arguments(x => x.Action);
        }

        public override bool Execute(StorageInput input)
        {
            using (var host = new JasperRuntime(input.BuildHost()))
            {
                var persistor = host.Get<IEnvelopePersistence>();

                persistor.Describe(Console.Out);

                switch (input.Action)
                {
                    case (StorageAction.counts):

                        var counts = persistor.Admin.GetPersistedCounts().GetAwaiter().GetResult();
                        Console.WriteLine("Persisted Enveloper Counts");
                        Console.WriteLine($"Incoming    {counts.Incoming.ToString().PadLeft(5)}");
                        Console.WriteLine($"Outgoing    {counts.Outgoing.ToString().PadLeft(5)}");
                        Console.WriteLine($"Scheduled   {counts.Scheduled.ToString().PadLeft(5)}");

                        break;

                    case (StorageAction.clear):
                        persistor.Admin.ClearAllPersistedEnvelopes();
                        ConsoleWriter.Write(ConsoleColor.Green, "Successfully deleted all persisted envelopes");
                        break;

                    case (StorageAction.rebuild):
                        persistor.Admin.RebuildSchemaObjects();
                        ConsoleWriter.Write(ConsoleColor.Green, "Successfully rebuilt the envelope storage");
                        break;

                    case (StorageAction.script):
                        Console.WriteLine("Exporting script to " + input.FileFlag.ToFullPath());
                        File.WriteAllText(input.FileFlag, persistor.Admin.CreateSql());

                        break;
                }
            }

            return true;
        }
    }

}
