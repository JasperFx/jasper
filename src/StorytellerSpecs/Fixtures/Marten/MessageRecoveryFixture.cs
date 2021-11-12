using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Persistence.Operations;
using Jasper.Persistence.Postgresql;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Stub;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Marten
{
    public class MessageRecoveryFixture : Fixture, IDurabilityAgent
    {
        private readonly IList<Envelope> _envelopes = new List<Envelope>();

        private readonly IList<NodeLocker> _nodeLockers = new List<NodeLocker>();

        private readonly LightweightCache<string, int> _owners = new LightweightCache<string, int>();
        private IEnvelopeStorageAdmin _admin;
        private int _currentNodeId;

        private IHost _host;
        private PostgresqlSettings _settings;
        private RecordingWorkerQueue _workers;
        private IDocumentStore theStore;

        public MessageRecoveryFixture()
        {
            Title = "Marten-backed Message Recovery";

            _owners["Any Node"] = TransportConstants.AnyNode;
            _owners["Other Node"] = -13234;
            _owners["Third Node"] = -13334;
            _owners["Fourth Node"] = -13335;

            Lists["channels"].AddValues("stub://one", "stub://two", "stub://three");
            Lists["status"].AddValues(EnvelopeStatus.Incoming.ToString(), EnvelopeStatus.Outgoing.ToString(),
                EnvelopeStatus.Scheduled.ToString());

            Lists["owners"].AddValues("This Node", "Other Node", "Any Node", "Third Node");
        }

        void IDurabilityAgent.RescheduleOutgoingRecovery()
        {
        }

        Task IDurabilityAgent.EnqueueLocally(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        void IDurabilityAgent.RescheduleIncomingRecovery()
        {
        }

        public override void SetUp()
        {
            _envelopes.Clear();
            _nodeLockers.Clear();

            _workers = new RecordingWorkerQueue();

            _host = Host.CreateDefaultBuilder()
                .UseJasper(_ =>
                {
                    _.ServiceName = Guid.NewGuid().ToString();

                    _.Extensions.UseMarten(Servers.PostgresConnectionString);

                    _.Services.AddSingleton<IWorkerQueue>(_workers);


                    _.Advanced.FirstNodeReassignmentExecution = 30.Minutes();
                    _.Advanced.ScheduledJobFirstExecution = 30.Minutes();
                    _.Advanced.FirstNodeReassignmentExecution = 30.Minutes();
                    _.Advanced.NodeReassignmentPollingTime = 30.Minutes();
                })
                .Start();


            _admin = _host.Services.GetService<IEnvelopePersistence>().Admin;
            _admin.RebuildSchemaObjects().GetAwaiter().GetResult();

            _settings = _host.Services.GetService<PostgresqlSettings>();

            theStore = _host.Services.GetService<IDocumentStore>();
            theStore.Advanced.Clean.DeleteAllDocuments();

            _currentNodeId = _host.Services.GetService<AdvancedSettings>().UniqueNodeId;

            _owners["This Node"] = _currentNodeId;
        }

        public override void TearDown()
        {
            _host.Dispose();

            foreach (var locker in _nodeLockers) locker.SafeDispose();

            _nodeLockers.Clear();
        }

        [ExposeAsTable("The persisted envelopes are")]
        public void EnvelopesAre(
            [Default("NULL")] string Note,
            Guid Id,
            [SelectionList("channels")] [Default("stub://one")]
            Uri Destination,
            [Default("NULL")] DateTime? ExecutionTime,
            [Default("TODAY+1")] DateTime DeliverBy,
            [SelectionList("status")] EnvelopeStatus Status,
            [SelectionList("owners")] string Owner)
        {
            var ownerId = _owners[Owner];

            var envelope = new Envelope
            {
                Id = Id,
                ExecutionTime = ExecutionTime,
                Status = Status,
                OwnerId = ownerId,
                DeliverBy = DeliverBy,
                Destination = Destination,
                Message = new Message1()
            };

            var writer = _host.Services.GetRequiredService<IMessagingRoot>().Options.Serializers
                .FirstOrDefault(x => x.ContentType == EnvelopeConstants.JsonContentType);
            envelope.Data = writer.Write(envelope.Message);
            envelope.ContentType = writer.ContentType;

            _envelopes.Add(envelope);
        }

        [FormatAs("Channel {channel} is unavailable and latched for sending")]
        public void ChannelIsLatched(Uri channel)
        {
            _host.GetStubTransport().Endpoints[channel].Latched = true;

            // Gotta do this so that the query on latched channels works correctly
            _host.Services.GetService<IMessagingRoot>().Runtime.GetOrBuildSendingAgent(channel);
        }


        private IList<OutgoingMessageAction> outgoingMessages()
        {
            var stub = getStubTransport();

            return stub.Endpoints.SelectMany(x =>
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
            return _host.GetStubTransport();
        }

        public IGrammar TheEnvelopesSentShouldBe()
        {
            return VerifySetOf(outgoingMessages).Titled("The envelopes sent should be")
                .MatchOn(x => x.Id, x => x.Destination);
        }

        private IReadOnlyList<Envelope> persistedEnvelopes(int ownerId)
        {
            return _admin.AllIncomingEnvelopes().GetAwaiter().GetResult()
                .Concat(_admin.AllOutgoingEnvelopes().GetAwaiter().GetResult())
                .Where(x => x.OwnerId == ownerId)
                .ToList();
        }

        public IGrammar ThePersistedEnvelopesOwnedByTheCurrentNodeAre()
        {
            return VerifySetOf(() => persistedEnvelopes(_currentNodeId))
                .Titled("The persisted envelopes owned by the current node should be")
                .MatchOn(x => x.Id);
        }

        public IGrammar ThePersistedEnvelopesOwnedByAnyNodeAre()
        {
            return VerifySetOf(() => persistedEnvelopes(TransportConstants.AnyNode))
                .Titled("The persisted envelopes owned by 'any' node should be")
                .MatchOn(x => x.Id);
        }

        public IGrammar TheProcessedEnvelopesShouldBe()
        {
            return VerifySetOf(() => _workers.Enqueued)
                .Titled("The envelopes enqueued to the worker queues should be")
                .MatchOn(x => x.Id);
        }

        [FormatAs("Node {node} is active")]
        public void NodeIsActive([SelectionList("owners")] string node)
        {
            var ownerId = _owners[node];
            _nodeLockers.Add(new NodeLocker(ownerId));
        }

        private async Task runAction<T>() where T : IMessagingAction
        {
            using (var session = theStore.LightweightSession())
            {
                foreach (var envelope in _envelopes)
                {
                    if (envelope.Status == EnvelopeStatus.Outgoing)
                    {
                        session.StoreOutgoing(_settings, envelope, envelope.OwnerId);
                    }
                    else
                    {
                        session.StoreIncoming(_settings, envelope);
                    }
                }

                await session.SaveChangesAsync();
            }

            var agent = DurabilityAgent.ForHost(_host);

            var action = _host.Get<T>();
            await agent.Execute(action);
        }

        [FormatAs("After reassigning envelopes from dormant nodes")]
        public Task AfterReassigningFromDormantNodes()
        {
            return runAction<NodeReassignment>();
        }

        [FormatAs("After recovering incoming messages")]
        public Task AfterRecoveringIncomingMessages()
        {
            return runAction<RecoverIncomingMessages>();
        }

        [FormatAs("After executing the outgoing message recovery")]
        public Task AfterExecutingTheOutgoingMessageRecovery()
        {
            return runAction<RecoverOutgoingMessages>();
        }
    }

    public class NodeLocker : IDisposable
    {
        private readonly NpgsqlConnection _conn;
        private readonly NpgsqlTransaction _tx;

        public NodeLocker(int nodeId)
        {
            _conn = new NpgsqlConnection(Servers.PostgresConnectionString);
            _conn.Open();
            _tx = _conn.BeginTransaction();

            new PostgresqlSettings().TryGetGlobalTxLock(_conn, _tx, nodeId).Wait(3.Seconds());
        }

        public void Dispose()
        {
            _tx.Rollback();
            _conn?.Dispose();
        }
    }

    public class OutgoingMessageAction
    {
        public Guid Id { get; set; }
        public Uri Destination { get; set; }
    }


    public class RecordingWorkerQueue : IWorkerQueue
    {
        public readonly IList<Envelope> Enqueued = new List<Envelope>();

        public Task Enqueue(Envelope envelope)
        {
            Enqueued.Add(envelope);
            return Task.CompletedTask;
        }

        public int QueuedCount => 5;

        public Task ScheduleExecution(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        void IWorkerQueue.StartListening(IListener listener)
        {
            throw new NotImplementedException();
        }


        Task IListeningWorkerQueue.Received(Uri uri, Envelope[] messages)
        {
            throw new NotImplementedException();
        }

        public Task Received(Uri uri, Envelope envelope)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
        }
    }
}
