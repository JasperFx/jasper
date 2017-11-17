using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Marten;
using Marten.Services;
using Marten.Util;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    public interface IMessagingAction
    {
        Task Execute(IDocumentSession session);
    }

    // This will be a singleton
    public class SchedulingAgent
    {
        private readonly IChannelGraph _channels;
        private readonly IWorkerQueue _workers;
        private readonly IDocumentStore _store;
        private readonly BusSettings _settings;
        private readonly CompositeLogger _logger;
        private readonly ActionBlock<IMessagingAction> _worker;
        private NpgsqlConnection _connection;

        public SchedulingAgent(IChannelGraph channels, IWorkerQueue workers, IDocumentStore store, BusSettings settings, CompositeLogger logger)
        {
            _channels = channels;
            _workers = workers;
            _store = store;
            _settings = settings;
            _logger = logger;

            _worker = new ActionBlock<IMessagingAction>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });
        }

        private async Task processAction(IMessagingAction action)
        {
            var tx = _connection.BeginTransaction();

            var session = _store.OpenSession(new SessionOptions
            {
                Connection = _connection,
                Transaction = tx,
                Tracking = DocumentTracking.None
            });

            try
            {
                await action.Execute(session);

                if (!tx.IsCompleted)
                {
                    await tx.RollbackAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
            finally
            {
                session.Dispose();
            }
        }

        public async Task Start()
        {
            _connection = _store.Tenancy.Default.CreateConnection();


            await _connection.OpenAsync(_settings.Cancellation);

            await retrieveLockForThisNode();
        }

        public async Task Stop()
        {
            _worker.Complete();

            await _worker.Completion;

            await _connection.CreateCommand().Sql("SELECT pg_advisory_unlock(:id)")
                .Sql("SELECT pg_advisory_lock(:id)")
                .With("id", _settings.UniqueNodeId, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync(_settings.Cancellation);

            _connection.Close();
            _connection.Dispose();
            _connection = null;


        }


        private Task retrieveLockForThisNode()
        {
            return _connection
                .CreateCommand()
                .Sql("SELECT pg_advisory_lock(:id)")
                .With("id", _settings.UniqueNodeId, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync(_settings.Cancellation);
        }

    }

    public class RecoverIncomingMessages : IMessagingAction
    {
        public Task Execute(IDocumentSession session)
        {
            // try to get the "jasper-incoming" lock
            // if so, pull 100 messages. Assign all to this node
            // put in worker queues

            throw new NotImplementedException();
        }
    }

    public class RecoverOutgoingMessages : IMessagingAction
    {
        public Task Execute(IDocumentSession session)
        {
            // try to get the "jasper-outgoing" lock
            // if so, pull 100 messages. Assign all to this node
            // put in sender agents
            // will need an IChannel.QuickSend()

            throw new NotImplementedException();
        }
    }

    public class ReassignFromDormantNodes : IMessagingAction
    {
        public Task Execute(IDocumentSession session)
        {
            // think this needs to be a sproc w/ a cursor. Boo.
            throw new NotImplementedException();
        }
    }
}
