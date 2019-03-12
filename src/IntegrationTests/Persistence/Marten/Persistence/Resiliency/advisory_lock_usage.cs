using System;
using System.Threading.Tasks;
using Jasper.Persistence.Postgresql.Util;
using Npgsql;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten.Persistence.Resiliency
{
    public class advisory_lock_usage : MartenContext
    {
        //[Fact] -- too slow
        public async Task tx_session_locks()
        {
            using (var conn1 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn2 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn3 = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();

                var tx1 = conn1.BeginTransaction();
                await conn1.GetGlobalTxLock(1);

                var locks = await conn1.GetOpenLocks();

                try
                {
                    // Cannot get the lock here
                    var tx2 = conn2.BeginTransaction();
                    (await conn2.TryGetGlobalTxLock(1)).ShouldBeFalse();

                    // Can get the new lock
                    var tx3 = conn3.BeginTransaction();
                    (await conn3.TryGetGlobalTxLock(2)).ShouldBeTrue();

                    // Cannot get the lock here
                    (await conn2.TryGetGlobalTxLock(2)).ShouldBeFalse();

                    await tx1.RollbackAsync();
                    await tx2.RollbackAsync();
                    await tx3.RollbackAsync();
                }
                finally
                {
                    await conn1.ReleaseGlobalLock(1);
                }
            }
        }

        //[Fact] - too slow
        public async Task global_session_locks()
        {
            using (var conn1 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn2 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn3 = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();

                await conn1.GetGlobalLock(1);

                var locks = await conn1.GetOpenLocks();

                try
                {
                    // Cannot get the lock here
                    (await conn2.TryGetGlobalLock(1)).ShouldBeFalse();

                    // Can get the new lock
                    (await conn3.TryGetGlobalLock(2)).ShouldBeTrue();

                    // Cannot get the lock here
                    (await conn2.TryGetGlobalLock(2)).ShouldBeFalse();
                }
                finally
                {
                    await conn1.ReleaseGlobalLock(1);
                }
            }
        }


        [Fact]
        public async Task explicitly_release_global_session_locks()
        {
            var lockNumber = new Random().Next();

            using (var conn1 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn2 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn3 = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();


                await conn1.GetGlobalLock(lockNumber);


                // Cannot get the lock here
                (await conn2.TryGetGlobalLock(lockNumber)).ShouldBeFalse();


                await conn1.ReleaseGlobalLock(lockNumber);


                for (var j = 0; j < 5; j++)
                {
                    if (await conn2.TryGetGlobalLock(lockNumber)) return;

                    await Task.Delay(250);
                }

                throw new Exception("Advisory lock was not released");
            }
        }

        [Fact]
        public async Task explicitly_release_global_tx_session_locks()
        {
            var lockNumber = new Random().Next();

            using (var conn1 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn2 = new NpgsqlConnection(Servers.PostgresConnectionString))
            using (var conn3 = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();

                var tx1 = conn1.BeginTransaction();
                await conn1.GetGlobalTxLock(lockNumber);


                // Cannot get the lock here
                var tx2 = conn2.BeginTransaction();
                (await conn2.TryGetGlobalTxLock(lockNumber)).ShouldBeFalse();


                await tx1.RollbackAsync();


                for (var j = 0; j < 5; j++)
                {
                    if (await conn2.TryGetGlobalTxLock(lockNumber))
                    {
                        await tx2.RollbackAsync();
                        return;
                    }

                    await Task.Delay(250);
                }

                throw new Exception("Advisory lock was not released");
            }
        }
    }
}
