using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BenchmarkDotNet.Attributes;
using IntegrationTests;
using Jasper;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TestMessages;

namespace Benchmarks
{
    [MemoryDiagnoser()]
    public class PersistenceRunner : IDisposable
    {
        private Envelope[] theEnvelopes;

        public PersistenceRunner()
        {
            theDriver = new Driver();
        }

        [Params("SqlServer", "Postgresql")] public string DatabaseEngine;

        public void Dispose()
        {
            theDriver.SafeDispose();
        }

        private readonly Driver theDriver;

        [IterationSetup]
        public void BuildDatabase()
        {
            theDriver.Start(opts =>
            {
                opts.Advanced.DurabilityAgentEnabled = false;
                if (DatabaseEngine == "SqlServer")
                {
                    opts.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
                }
                else
                {
                    opts.PersistMessagesWithPostgresql(Servers.PostgresConnectionString);
                }
            }).GetAwaiter().GetResult();

            theEnvelopes = theDriver.Targets.Select(x =>
            {
                var stream = new MemoryStream();
                var writer = new JsonTextWriter(new StreamWriter(stream));
                new JsonSerializer().Serialize(writer, x);
                var env = new Envelope(x);
                env.Destination = new Uri("fake://localhost:5000");
                stream.Position = 0;
                env.Data = stream.ReadAllBytes();

                env.ContentType = EnvelopeConstants.JsonContentType;
                env.MessageType = "target";


                return env;
            }).ToArray();

        }

        [IterationCleanup]
        public void Teardown()
        {
            theDriver.Teardown().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task StoreIncoming()
        {
            for (var i = 0; i < 10; i++)
            {
                await theDriver.Persistence.StoreIncomingAsync(theEnvelopes.Skip(i * 100).Take(100).ToArray());
            }
        }

        [Benchmark]
        public async Task StoreOutgoing()
        {
            for (var i = 0; i < 10; i++)
            {
                await theDriver.Persistence.StoreOutgoingAsync(theEnvelopes.Skip(i * 100).Take(100).ToArray(), 5);
            }
        }

        [IterationSetup(Target = nameof(LoadIncoming))]
        public void LoadIncomingSetup()
        {
            BuildDatabase();
            StoreIncoming().GetAwaiter().GetResult();
        }

        [Benchmark]
        public Task LoadIncoming()
        {
            return theDriver.Persistence.Admin.AllIncomingAsync();
        }

        [IterationSetup(Target = nameof(LoadOutgoing))]
        public void LoadOutgoingSetup()
        {
            BuildDatabase();
            StoreOutgoing().GetAwaiter().GetResult();
        }

        [Benchmark]
        public Task LoadOutgoing()
        {
            return theDriver.Persistence.Admin.AllOutgoingAsync();
        }
    }

}
