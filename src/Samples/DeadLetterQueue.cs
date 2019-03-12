using System;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;

namespace Jasper.Testing.Samples
{
    public class DeadLetterQueue
    {
        // SAMPLE: FetchErrorReport
        public async Task load_error_report(IEnvelopePersistence persistence, Guid envelopeId)
        {
            var report = await persistence.Admin.LoadDeadLetterEnvelope(envelopeId);

            // The Id
            Console.WriteLine(report.Id);

            // Why it was moved out
            Console.WriteLine(report.Explanation);

            // The underlying message typ
            Console.WriteLine(report.MessageType);


            // Reconstitute the original Envelope
            // Envelope.Data would have the raw data here
            var envelope = report.RebuildEnvelope();

            // The name ofthe system that sent the message
            Console.WriteLine(report.Source);

            // The .Net Exception type name
            Console.WriteLine(report.ExceptionType);

            // Just the message of the exception
            Console.WriteLine(report.ExceptionMessage);

            // JUST SHOW ME THE FULL STACKTRACE ALREADY!!!!
            Console.WriteLine(report.ExceptionText);
        }

        // ENDSAMPLE
    }
}
