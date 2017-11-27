using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Stub;
using Jasper.Marten;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Marten.Tests.Setup;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace DurabilitySpecs.Fixtures.Marten
{
    public class RecoverOutgoingMessagesFixture : Fixture
    {
        private readonly IList<Envelope> _envelopes = new List<Envelope>();
        private int _currentNodeId;

        private JasperRuntime _runtime;
        private IDocumentStore theStore;

        public RecoverOutgoingMessagesFixture()
        {
            Title = "Marten-backed outgoing message recovery";

            Lists["channels"].AddValues("stub://one", "stub://two", "stub://three");
            Lists["status"].AddValues(TransportConstants.Incoming, TransportConstants.Outgoing,
                TransportConstants.Scheduled);

            Lists["owners"].AddValues("This Node", "Other Node", "Any Node");
        }

        public override void SetUp()
        {
            _envelopes.Clear();

            _runtime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
                _.Services.AddSingleton<ITransport, StubTransport>();
            });

            theStore = _runtime.Get<IDocumentStore>();
            theStore.Advanced.Clean.DeleteAllDocuments();

            _currentNodeId = _runtime.Get<BusSettings>().UniqueNodeId;
        }

        public override void TearDown()
        {
            _runtime.Dispose();
        }

        [ExposeAsTable("The persisted envelopes are")]
        public void EnvelopesAre(
            string Id,
            [SelectionList("channels")] Uri Destination,
            [Default("NULL")] DateTime? ExecutionTime,
            [Default("TODAY+1")] DateTime DeliverBy,
            [SelectionList("status")] string Status,
            [SelectionList("owners")] string Owner)
        {
            var ownerId = TransportConstants.AnyNode;
            switch (Owner)
            {
                case "Other Node":
                    ownerId = -14123453;
                    break;

                case "This Node":
                    ownerId = _currentNodeId;
                    break;
            }

            var envelope = new Envelope
            {
                Id = Id,
                ExecutionTime = ExecutionTime,
                Status = Status,
                OwnerId = ownerId,
                DeliverBy = DeliverBy,
                Destination = Destination
            };

            _envelopes.Add(envelope);
        }

        [FormatAs("Channel {channel} is unavailable and latched for sending")]
        public void ChannelIsLatched(Uri channel)
        {
            getStubTransport().Channels[channel].Latched = true;
        }

        [FormatAs("After executing the outgoing message recovery")]
        public async Task AfterExecutingTheOutgoingMessageRecovery()
        {
            theStore.BulkInsert(_envelopes.ToArray());

            var outgoing = _runtime.Get<RecoverOutgoingMessages>();
            using (var session = theStore.LightweightSession())
            {
                await outgoing.Execute(session);
            }
        }

        private IList<OutgoingMessageAction> outgoingMessages()
        {
            var stub = getStubTransport();

            return stub.Channels.SelectMany(x =>
            {
                return x.Sent.Select(c => new OutgoingMessageAction
                {
                    Id = c.Id,
                    Destination = x.Destination
                });
            }).ToList();
        }

        private StubTransport getStubTransport()
        {
            var stub = _runtime.Container.GetAllInstances<ITransport>().OfType<StubTransport>().Single();
            return stub;
        }

        public IGrammar TheEnvelopesSentShouldBe()
        {
            return VerifySetOf(outgoingMessages).Titled("The envelopes sent should be")
                .MatchOn(x => x.Id, x => x.Destination);
        }

        private IReadOnlyList<Envelope> persistedEnvelopes(int ownerId)
        {
            using (var session = theStore.QuerySession())
            {
                return session.Query<Envelope>().Where(x => x.OwnerId == ownerId).ToList();
            }
        }

        public IGrammar ThePersistedEnvelopesOwnedByTheCurrentNodeAre()
        {
            return VerifySetOf(() => persistedEnvelopes(_currentNodeId))
                .Titled("The persisted envelopes owned by the current node should be")
                .MatchOn(x => x.Id);
        }

        public IGrammar ThePersistedEnvelopesOwnedByAnyNodeAre()
        {
            return VerifySetOf(() => persistedEnvelopes(_currentNodeId))
                .Titled("The persisted not owned by the current node are")
                .MatchOn(x => x.Id);
        }
    }

    public class OutgoingMessageAction
    {
        public string Id { get; set; }
        public Uri Destination { get; set; }
    }
}
