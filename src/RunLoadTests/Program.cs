using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.DbObjects;
using Marten;
using Oakton;
using TestMessages;

namespace RunLoadTests
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return CommandExecutor.For(x =>
            {
                x.RegisterCommands(typeof(Program).Assembly);
            }).ExecuteAsync(args);
        }
    }

    public class PostgresInput
    {
        public string ConnectionFlag { get; set; } =
            "Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres";

        public IDocumentStore StoreForSchema(string schemaName)
        {
            return DocumentStore.For(x =>
            {
                x.Connection(ConnectionFlag);
                x.PLV8Enabled = false;
                x.AutoCreateSchemaObjects = AutoCreate.All;

                x.Storage.Add<PostgresqlEnvelopeStorage>();
                x.Schema.For<SentTrack>();
                x.Schema.For<ReceivedTrack>();
            });
        }
    }

    public class SqlInput
    {
        public string ConnectionFlag { get; set; } =
            "Server=localhost;Database=jasper_testing;User Id=sa;Password=P@55w0rd";


    }
}
