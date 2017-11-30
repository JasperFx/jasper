using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Bus.Logging
{
    public class ConsoleTransportLogger : ITransportLogger
    {
        public void OutgoingBatchSucceeded(OutgoingMessageBatch batch)
        {
            Console.WriteLine($"Successfully sent {batch.Messages.Count} messages to {batch.Destination}");
        }

        public void IncomingBatchReceived(IEnumerable<Envelope> envelopes)
        {
            Console.WriteLine($"Successfully received {envelopes.Count()} messages");
        }

        public void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null)
        {
            ConsoleWriter.Write(ConsoleColor.Red, $"Failed to send outgoing envelopes batch to {batch.Destination}");
            if (ex != null) ConsoleWriter.Write(ConsoleColor.Yellow, ex.ToString());
            Console.WriteLine();
        }

        public void CircuitBroken(Uri destination)
        {
            ConsoleWriter.Write(ConsoleColor.Red, $"Sending agent for {destination} is latched");
        }

        public void CircuitResumed(Uri destination)
        {
            ConsoleWriter.Write(ConsoleColor.Green, $"Sending agent for {destination} has resumed");
        }

        public void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                Console.WriteLine($"Enqueued scheduled job {envelope} locally");
            }
        }

        public void RecoveredIncoming(IEnumerable<Envelope> envelopes)
        {
            Console.WriteLine($"Recovered {envelopes.Count()} incoming envelopes from storage");
        }

        public void RecoveredOutgoing(IEnumerable<Envelope> envelopes)
        {
            Console.WriteLine($"Recovered {envelopes.Count()} outgoing envelopes from storage");
        }

        public void DiscardedExpired(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                Console.WriteLine($"Discarded expired envelope {envelope}");
            }
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
            ConsoleWriter.Write(ConsoleColor.Red, message);

            if (correlationId.IsNotEmpty())
            {
                ConsoleWriter.Write(ConsoleColor.Red, $"Id: {correlationId}");
            }

            ConsoleWriter.Write(ConsoleColor.Yellow, ex.ToString());
            Console.WriteLine();
        }

        public void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                ConsoleWriter.Write(ConsoleColor.Yellow, $"Discarded {envelope} with unknown transport");
            }
        }
    }
}
