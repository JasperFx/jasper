using System;
using System.Collections.Generic;
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
    public class PersistenceRunner : IDisposable
    {
        private readonly Target[] _targets;
        private IHost theSystem;
        private Envelope[] theEnvelopes;
        private IEnvelopePersistence thePersistor;

        public PersistenceRunner()
        {
            var json = File.ReadAllText("targets.json");
            _targets = JsonConvert.DeserializeObject<Target[]>(json);


        }

        public void Dispose()
        {
            theSystem.SafeDispose();
        }

        [Params("SqlServer", "Postgresql")]
        public string DatabaseEngine;

        [IterationSetup]
        public void BuildDatabase()
        {
            theSystem = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    if (DatabaseEngine == "SqlServer")
                    {
                        opts.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
                    }
                    else
                    {
                        opts.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString);
                    }

                })
                .Build();

            theSystem.RebuildMessageStorage().GetAwaiter().GetResult();

            theEnvelopes = _targets.Select(x =>
            {
                var stream = new MemoryStream();
                var writer = new JsonTextWriter(new StreamWriter(stream));
                new JsonSerializer().Serialize(writer, x);
                var env = new Envelope(x);
                env.Destination = new Uri("fake://localhost:5000");
                stream.Position = 0;
                env.Data = stream.ReadAllBytes();


                return env;
            }).ToArray();

            thePersistor = theSystem.Services.GetRequiredService<IEnvelopePersistence>();
        }

        [IterationCleanup]
        public void Teardown()
        {
            theSystem.Dispose();
        }

        [Benchmark]
        public async Task StoreIncoming()
        {
            for (int i = 0; i < 10; i++)
            {
                await thePersistor.StoreIncoming(theEnvelopes.Skip(i * 100).Take(100).ToArray());
            }
        }

        [Benchmark]
        public async Task StoreOutgoing()
        {
            for (int i = 0; i < 10; i++)
            {
                await thePersistor.Outgoing.StoreOutgoing(theEnvelopes.Skip(i * 100).Take(100).ToArray(), 5);
            }
        }

        [IterationSetup(Target = nameof(LoadIncoming))]
        public Task LoadIncomingSetup()
        {
            return StoreIncoming();
        }

        public Task LoadIncoming()
        {
            return thePersistor.Admin.AllIncomingEnvelopes();
        }

        [IterationSetup(Target = nameof(LoadOutgoing))]
        public Task LoadOutgoingSetup()
        {
            return StoreOutgoing();
        }

        public Task LoadOutgoing()
        {
            return thePersistor.Admin.AllOutgoingEnvelopes();
        }

    }

    /*
     * TODO
     * -- build out a 1,000 envelopes
     * -- option for envelopes to be pre-serialized too
     * -- methods to load the envelopes too, both incoming and outgoing
     * -- Refactor all the envelope loading to EnveloperPersistence. Move common elements to DatabaseBackedEnvelopePersistence
     */
}
