using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Marten;
using Marten.Services;
using Marten.Util;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class SchedulingAgent : IHostedService, IDisposable, ISchedulingAgent
    {
        private readonly IChannelGraph _channels;
        private readonly IWorkerQueue _workers;
        private readonly IDocumentStore _store;
        private readonly MessagingSettings _settings;
        private readonly ITransportLogger _logger;
        private readonly StoreOptions _storeOptions;
        private readonly ActionBlock<IMessagingAction> _worker;
        private NpgsqlConnection _connection;
        private readonly RunScheduledJobs _scheduledJobs;
        private readonly RecoverIncomingMessages _incomingMessages;
        private readonly RecoverOutgoingMessages _outgoingMessages;
        private Timer _scheduledJobTimer;
        private Timer _nodeReassignmentTimer;
        private readonly ReassignFromDormantNodes _nodeReassignment;

        public SchedulingAgent(IChannelGraph channels, IWorkerQueue workers, IDocumentStore store, MessagingSettings settings, ITransportLogger logger, StoreOptions storeOptions, IRetries retries)
        {
            _channels = channels;
            _workers = workers;
            _store = store;
            _settings = settings;
            _logger = logger;
            _storeOptions = storeOptions;

            _worker = new ActionBlock<IMessagingAction>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });

            var marker = new EnvelopeTables(_settings, _storeOptions);
            _scheduledJobs = new RunScheduledJobs(_workers, _store, marker, logger, retries);
            _incomingMessages = new RecoverIncomingMessages(_workers, _settings, marker, this, _logger);
            _outgoingMessages = new RecoverOutgoingMessages(_channels, _settings, marker, this, _logger);

            _nodeReassignment = new ReassignFromDormantNodes(marker, settings);
        }

        public void RescheduleOutgoingRecovery()
        {
            _worker.Post(_outgoingMessages);
        }

        public void RescheduleIncomingRecovery()
        {
            _worker.Post(_incomingMessages);
        }


        public void Dispose()
        {
            _connection?.Dispose();
            _scheduledJobTimer?.Dispose();
            _nodeReassignmentTimer?.Dispose();
        }

        private async Task processAction(IMessagingAction action)
        {
            await tryRestartConnection();

            if (_connection == null) return;

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


            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
            finally
            {
                if (!tx.IsCompleted)
                {
                    await tx.RollbackAsync();
                }

                session.Dispose();
            }

            await tryRestartConnection();
        }

        private async Task tryRestartConnection()
        {
            if (_connection?.State == ConnectionState.Open) return;

            if (_connection != null)
            {
                try
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
                catch (Exception e)
                {
                    _logger.LogException(e);
                }
            }


                _connection = _store.Tenancy.Default.CreateConnection();

            try
            {
                await _connection.OpenAsync(_settings.Cancellation);

                await retrieveLockForThisNode();
            }
            catch (Exception e)
            {
                _logger.LogException(e);

                _connection.Dispose();
                _connection = null;
            }

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));

            _connection = _store.Tenancy.Default.CreateConnection();


            await _connection.OpenAsync(_settings.Cancellation);

            await retrieveLockForThisNode();

            _scheduledJobTimer = new Timer(s =>
            {
                _worker.Post(_scheduledJobs);
                _worker.Post(_incomingMessages);
                _worker.Post(_outgoingMessages);

            }, _settings, _settings.FirstScheduledJobExecution, _settings.ScheduledJobPollingTime);

            _nodeReassignmentTimer = new Timer(s =>
            {
                _worker.Post(_nodeReassignment);


            }, _settings, _settings.FirstNodeReassignmentExecution, _settings.NodeReassignmentPollingTime);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _worker.Complete();

            await _worker.Completion;

            await _connection.CreateCommand().Sql("SELECT pg_advisory_unlock(:id)")
                .Sql("SELECT pg_advisory_lock(:id)")
                .With("id", _settings.UniqueNodeId, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync(CancellationToken.None);

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
}
