using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Marten.Resiliency
{
    public class SchedulingAgent : SchedulingAgentBase<IMessagingAction>
    {
        private readonly IDocumentStore _store;

        private NpgsqlConnection _connection;


        public SchedulingAgent(IChannelGraph channels, IWorkerQueue workers, IDocumentStore store, MessagingSettings settings, ITransportLogger logger, StoreOptions storeOptions, IRetries retries, EnvelopeTables tables)
            : base(settings, logger,
                new RunScheduledJobs(workers, store, tables, logger, retries),
                new RecoverIncomingMessages(workers, settings, tables, logger),
                new RecoverOutgoingMessages(channels, settings, tables, logger),
                new ReassignFromDormantNodes(tables, settings)


                )
        {
            _store = store;

        }


        protected override void disposeConnection()
        {
            _connection?.ReleaseGlobalLock(settings.UniqueNodeId).Wait(5.Seconds());
            _connection?.Dispose();
        }

        protected override async Task processAction(IMessagingAction action)
        {
            try
            {
                await tryRestartConnection();

                if (_connection == null) return;

                var tx = _connection.BeginTransaction();

                var session = _store.OpenSession(new SessionOptions
                {
                    Connection = _connection,
                    Transaction = tx,
                    Tracking = DocumentTracking.None,
                    OwnsTransactionLifecycle = true
                });

                try
                {
                    await action.Execute(session, this);


                }
                catch (Exception e)
                {
                    logger.LogException(e);
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
            catch (Exception e)
            {
                logger.LogException(e);
            }
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
                    logger.LogException(e);
                }
            }


                _connection = _store.Tenancy.Default.CreateConnection();

            try
            {
                await _connection.OpenAsync(settings.Cancellation);

                await retrieveLockForThisNode();
            }
            catch (Exception e)
            {
                logger.LogException(e);

                _connection.Dispose();
                _connection = null;
            }

        }



        protected override async Task openConnectionAndAttainNodeLock()
        {
            _store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));

            _connection = _store.Tenancy.Default.CreateConnection();

            await _connection.OpenAsync(settings.Cancellation);

            await retrieveLockForThisNode();
        }



        protected override async Task releaseNodeLockAndClose()
        {
            Trace.WriteLine($"Releasing lock for node {settings.UniqueNodeId} in service {settings.ServiceName}");

            await _connection.ReleaseGlobalLock(settings.UniqueNodeId);

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }


        private Task retrieveLockForThisNode()
        {
            Trace.WriteLine($"Attaining lock for node {settings.UniqueNodeId} in service {settings.ServiceName}");

            return _connection.GetGlobalLock(settings.UniqueNodeId);
        }

    }
}
