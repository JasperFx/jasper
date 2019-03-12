using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public static class SqlServerEnvelopeStorageExtensions
    {
        public static async Task<Envelope[]> ExecuteToEnvelopes(this SqlCommand command, SqlTransaction tx = null)
        {
            if (tx != null) command.Transaction = tx;
            using (var reader = await command.ExecuteReaderAsync())
            {
                var list = new List<Envelope>();

                while (await reader.ReadAsync())
                {
                    var bytes = await reader.GetFieldValueAsync<byte[]>(0);
                    var envelope = Envelope.Deserialize(bytes);

                    if (reader.FieldCount == 3)
                    {
                        envelope.Status = await reader.GetFieldValueAsync<string>(1);
                        envelope.OwnerId = await reader.GetFieldValueAsync<int>(2);
                    }

                    list.Add(envelope);
                }

                return list.ToArray();
            }
        }

        public static Envelope[] LoadEnvelopes(this SqlCommand command, SqlTransaction tx = null)
        {
            if (tx != null) command.Transaction = tx;
            using (var reader = command.ExecuteReader())
            {
                var list = new List<Envelope>();

                while (reader.Read())
                {
                    var bytes = reader.GetFieldValue<byte[]>(0);
                    var envelope = Envelope.Deserialize(bytes);
                    envelope.Status = reader.GetFieldValue<string>(1);
                    envelope.OwnerId = reader.GetFieldValue<int>(2);


                    if (!reader.IsDBNull(3))
                    {
                        var raw = reader.GetFieldValue<DateTimeOffset>(3);


                        envelope.ExecutionTime = raw.ToUniversalTime();
                    }

                    if (reader.FieldCount >= 5)
                        envelope.Attempts = reader.GetFieldValue<int>(4);


                    list.Add(envelope);
                }

                return list.ToArray();
            }
        }
    }
}
