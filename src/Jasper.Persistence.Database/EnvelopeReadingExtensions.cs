using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Persistence.Database
{
    public static class EnvelopeReadingExtensions
    {
        public static async Task<Envelope[]> ExecuteToEnvelopes(this DbCommand command, CancellationToken cancellation = default(CancellationToken),
            DbTransaction tx = null)
        {
            if (tx != null) command.Transaction = tx;
            using (var reader = await command.ExecuteReaderAsync(cancellation))
            {
                var list = new List<Envelope>();

                while (await reader.ReadAsync(cancellation))
                {
                    var bytes = await reader.GetFieldValueAsync<byte[]>(0, cancellation);
                    var envelope = Envelope.Deserialize(bytes);

                    if (reader.FieldCount == 2)
                    {
                        envelope.OwnerId = await reader.GetFieldValueAsync<int>(1, cancellation);
                    }
                    else if (reader.FieldCount == 3)
                    {
                        envelope.Status = await reader.GetFieldValueAsync<string>(1, cancellation);
                        envelope.OwnerId = await reader.GetFieldValueAsync<int>(2, cancellation);
                    }

                    list.Add(envelope);
                }

                return list.ToArray();
            }
        }

        public static Envelope[] LoadEnvelopes(this DbCommand command, DbTransaction tx = null)
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
