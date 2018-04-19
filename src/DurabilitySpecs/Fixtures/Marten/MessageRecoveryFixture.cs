using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Marten;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Stub;
using Jasper.Messaging.WorkerQueues;
using Jasper.Testing.Messaging.Runtime;
using Marten;
using Marten.Util;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace DurabilitySpecs.Fixtures.Marten
{
    public class MessageRecoveryFixture : Fixture, ISchedulingAgent
    {
        private readonly IList<Envelope> _envelopes = new List<Envelope>();
        private int _currentNodeId;

        private JasperRuntime _runtime;
        private IDocumentStore theStore;
        private RecordingWorkerQueue _workers;

        private readonly LightweightCache<string, int> _owners = new LightweightCache<string, int>();

        public MessageRecoveryFixture()
        {
            Title = "Marten-backed Message Recovery";

            _owners["Any Node"] = TransportConstants.AnyNode;
            _owners["Other Node"] = -13234;
            _owners["Third Node"] = -13334;
            _owners["Fourth Node"] = -13335;

            Lists["channels"].AddValues("stub://one", "stub://two", "stub://three");
            Lists["status"].AddValues(TransportConstants.Incoming, TransportConstants.Outgoing,
                TransportConstants.Scheduled);

            Lists["owners"].AddValues("This Node", "Other Node", "Any Node", "Third Node");
        }

        public override void SetUp()
        {
            _envelopes.Clear();
            _nodeLockers.Clear();

            _workers = new RecordingWorkerQueue();
            _schedulerAgent = new RecordingSchedulingAgent();

            _runtime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
                _.Services.AddSingleton<ITransport, StubTransport>();

                _.Services.AddSingleton<IWorkerQueue>(_workers);
                _.Services.AddSingleton<ISchedulingAgent>(_schedulerAgent);

                _.Include<MartenBackedPersistence>();

                _.Settings.Alter<MessagingSettings>(x =>
                {
                    x.FirstNodeReassignmentExecution = 30.Minutes();
                    x.FirstScheduledJobExecution = 30.Minutes();
                    x.FirstNodeReassignmentExecution = 30.Minutes();
                    x.NodeReassignmentPollingTime = 30.Minutes();
                });

            });

            _runtime.Get<MartenBackedDurableMessagingFactory>().ClearAllStoredMessages();

            _marker = _runtime.Get<EnvelopeTables>();
            _serializers = _runtime.Get<MessagingSerializationGraph>();

            theStore = _runtime.Get<IDocumentStore>();
            theStore.Advanced.Clean.DeleteAllDocuments();

            _currentNodeId = _runtime.Get<MessagingSettings>().UniqueNodeId;

            _owners["This Node"] = _currentNodeId;
        }

        public override void TearDown()
        {
            _runtime.Dispose();

            foreach (var locker in _nodeLockers)
            {
                locker.SafeDispose();
            }

            _nodeLockers.Clear();
        }

        [ExposeAsTable("The persisted envelopes are")]
        public void EnvelopesAre(
            [Default("NULL")]string Note,
            Guid Id,
            [SelectionList("channels"), Default("stub://one")] Uri Destination,
            [Default("NULL")] DateTime? ExecutionTime,
            [Default("TODAY+1")] DateTime DeliverBy,
            [SelectionList("status")] string Status,
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

            var writer = _serializers.JsonWriterFor(envelope.Message.GetType());
            envelope.Data = writer.Write(envelope.Message);
            envelope.ContentType = writer.ContentType;

            _envelopes.Add(envelope);
        }

        [FormatAs("Channel {channel} is unavailable and latched for sending")]
        public void ChannelIsLatched(Uri channel)
        {


            getStubTransport().Channels[channel].Latched = true;

            // Gotta do this so that the query on latched channels works correctly
            _runtime.Get<IChannelGraph>().GetOrBuildChannel(channel);
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
                return session.AllIncomingEnvelopes().Concat(session.AllOutgoingEnvelopes())
                    .Where(x => x.OwnerId == ownerId)
                    .ToList();
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

        private readonly IList<NodeLocker> _nodeLockers = new List<NodeLocker>();
        private RecordingSchedulingAgent _schedulerAgent;
        private EnvelopeTables _marker;
        private MessagingSerializationGraph _serializers;

        [FormatAs("Node {node} is active")]
        public void NodeIsActive([SelectionList("owners")]string node)
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
                    if (envelope.Status == TransportConstants.Outgoing)
                    {
                        session.StoreOutgoing(_marker, envelope, envelope.OwnerId);
                    }
                    else
                    {
                        session.StoreIncoming(_marker, envelope);
                    }
                }

                await session.SaveChangesAsync();
            }

            var action = _runtime.Get<T>();
            using (var session = theStore.LightweightSession())
            {
                try
                {
                    await action.Execute(session, this);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                }
            }
        }

        [FormatAs("After reassigning envelopes from dormant nodes")]
        public Task AfterReassigningFromDormantNodes()
        {
            return runAction<ReassignFromDormantNodes>();
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

        void ISchedulingAgent.RescheduleOutgoingRecovery()
        {

        }

        void ISchedulingAgent.RescheduleIncomingRecovery()
        {
        }
    }

    public class NodeLocker : IDisposable
    {
        private readonly NpgsqlConnection _conn;
        private readonly NpgsqlTransaction _tx;

        public NodeLocker(int nodeId)
        {
            _conn = new NpgsqlConnection(ConnectionSource.ConnectionString);
            _conn.Open();
            _tx = _conn.BeginTransaction();

            _conn.TryGetGlobalTxLock(nodeId).Wait(3.Seconds());
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

    public class RecordingSchedulingAgent : ISchedulingAgent
    {
        public void RescheduleOutgoingRecovery()
        {

        }

        public void RescheduleIncomingRecovery()
        {

        }
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

        public void AddQueue(string queueName, int parallelization)
        {

        }

        public IScheduledJobProcessor ScheduledJobs
        {
            get
            {
                return new InMemoryScheduledJobProcessor(this);
            }
        }
    }
}
