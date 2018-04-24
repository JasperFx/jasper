using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public class advisory_lock_usage
    {
        [Fact] // -- too slow
        public async Task tx_session_locks()
        {
            using (var conn1 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn2 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn3 = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();

                var tx1 = conn1.BeginTransaction();
                await conn1.GetGlobalTxLock(tx1, 1);



                    // Cannot get the lock here
                    var tx2 = conn2.BeginTransaction();
                    (await conn2.TryGetGlobalTxLock(tx2, 1)).ShouldBeFalse();

                    // Can get the new lock
                    var tx3 = conn3.BeginTransaction();
                    (await conn3.TryGetGlobalTxLock(tx3, 2)).ShouldBeTrue();

                    // Cannot get the lock here
                    (await conn2.TryGetGlobalTxLock(tx2, 2)).ShouldBeFalse();

                    tx1.Rollback();
                    tx2.Rollback();
                    tx3.Rollback();
            }

        }

        [Fact] // - too slow
        public async Task global_session_locks()
        {
            using (var conn1 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn2 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn3 = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();

                await conn1.GetGlobalLock(1);


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
        public async Task explicitly_release_global_tx_session_locks()
        {
            using (var conn1 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn2 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn3 = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();

                var tx1 = conn1.BeginTransaction();
                await conn1.GetGlobalTxLock(tx1, 1);


                // Cannot get the lock here
                var tx2 = conn2.BeginTransaction();
                (await conn2.TryGetGlobalTxLock(tx2, 1)).ShouldBeFalse();


                tx1.Rollback();


                for (int j = 0; j < 5; j++)
                {
                    if ((await conn2.TryGetGlobalTxLock(tx2, 1)))
                    {
                        tx2.Rollback();
                        return;
                    }

                    await Task.Delay(250);
                }

                throw new Exception("Advisory lock was not released");

            }

        }


        [Fact]
        public async Task explicitly_release_global_session_locks()
        {
            using (var conn1 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn2 = new SqlConnection(ConnectionSource.ConnectionString))
            using (var conn3 = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn1.OpenAsync();
                await conn2.OpenAsync();
                await conn3.OpenAsync();


                await conn1.GetGlobalLock(1);


                // Cannot get the lock here
                (await conn2.TryGetGlobalLock(1)).ShouldBeFalse();



                await conn1.ReleaseGlobalLock(1);


                for (int j = 0; j < 5; j++)
                {
                    if ((await conn2.TryGetGlobalLock(1)))
                    {
                        return;
                    }

                    await Task.Delay(250);
                }

                throw new Exception("Advisory lock was not released");

            }

        }


    }
}
