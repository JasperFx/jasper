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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TestMessages;

namespace Benchmarks
{
    public class PersistenceRunner : IDisposable
    {
        private readonly Target[] _targets;
        private IHost thePostgresqlSystem;
        private Envelope[] theEnvelopes;
        private IEnvelopePersistence thePersistor;

        public PersistenceRunner()
        {
            var json = File.ReadAllText("targets.json");
            _targets = JsonConvert.DeserializeObject<Target[]>(json);


        }

        public void Dispose()
        {
            thePostgresqlSystem.SafeDispose();
        }

        [GlobalSetup]
        public async Task BuildDatabase()
        {
            thePostgresqlSystem = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString);
                })
                .StartAsync();

            await thePostgresqlSystem.RebuildMessageStorage();

            theEnvelopes = _targets.Select(x =>
            {
                var env = new Envelope(x);

                // TODO -- use an option that pre-serializes

                return env;
            }).ToArray();

            thePersistor = thePostgresqlSystem.Services.GetRequiredService<IEnvelopePersistence>();
        }

        // TODO -- setup the setups
        public Task StoreIncoming()
        {
            return thePersistor.StoreIncoming(theEnvelopes);
        }

        public Task StoreOutgoing()
        {
            return thePersistor.Outgoing.StoreOutgoing(theEnvelopes, 5);
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
