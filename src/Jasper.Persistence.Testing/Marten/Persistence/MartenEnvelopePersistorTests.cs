﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Postgresql;
using Jasper.Tcp;
using Jasper.Tracking;
using Marten;
using Microsoft.Extensions.Hosting;
using Oakton.Resources;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class MartenEnvelopePersistorTests : PostgresqlContext, IDisposable, IAsyncLifetime
    {
        public MartenEnvelopePersistorTests()
        {

        }

        public void Dispose()
        {
            theHost?.Dispose();
        }

        public async Task InitializeAsync()
        {
            var store = theHost.Get<IDocumentStore>();
            await store.Advanced.Clean.CompletelyRemoveAllAsync();
            await theHost.ResetResourceState();
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public IHost theHost = JasperHost.For(opts =>
        {
            opts.Services.AddMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "receiver";
            }).IntegrateWithJasper();

            opts.ListenAtPort(2345).DurablyPersistedLocally();
        });

        [Fact]
        public async Task get_counts()
        {
            var thePersistor = theHost.Get<PostgresqlEnvelopePersistence>();

            var list = new List<Envelope>();

            // 10 incoming
            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncomingAsync(list.ToArray());


            // 7 scheduled
            list.Clear();
            for (var i = 0; i < 7; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Scheduled;

                list.Add(envelope);
            }

            await thePersistor.StoreIncomingAsync(list.ToArray());


            // 3 outgoing
            list.Clear();
            for (var i = 0; i < 3; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoingAsync(list.ToArray(), 0);

            var counts = await thePersistor.Admin.FetchCountsAsync();

            counts.Incoming.ShouldBe(10);
            counts.Scheduled.ShouldBe(7);
            counts.Outgoing.ShouldBe(3);
        }
    }
}
