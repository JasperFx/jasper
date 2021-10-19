using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Transports;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public partial class DatabaseBackedEnvelopePersistence
    {
        public Task ScheduleExecution(Envelope[] envelopes)
        {
            var builder = DatabaseSettings.ToCommandBuilder();

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id);
                var time = builder.AddParameter(envelope.ExecutionTime.Value);
                var attempts = builder.AddParameter(envelope.Attempts);

                builder.Append(
                    $"update {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} set execution_time = @{time.ParameterName}, status = \'{EnvelopeStatus.Scheduled}\', attempts = @{attempts.ParameterName}, owner_id = {TransportConstants.AnyNode} where id = @{id.ParameterName};");
            }

            return builder.Compile().ExecuteOnce(_cancellation);
        }


        public Task ScheduleJob(Envelope envelope)
        {
            envelope.Status = EnvelopeStatus.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            return StoreIncoming(envelope);
        }


        public Task<IReadOnlyList<Envelope>> LoadScheduledToExecute(DateTimeOffset utcNow)
        {
            return Session.Transaction
                .CreateCommand($"select {DatabaseConstants.IncomingFields} from {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where status = '{EnvelopeStatus.Scheduled}' and execution_time <= @time")
                .With("time", utcNow)
                .FetchList(r => ReadIncoming(r, _cancellation), cancellation: _cancellation);
        }



    }
}
