using System.Data.SqlClient;
using Baseline.Dates;
using Jasper.Messaging.ErrorHandling;
using Polly;

namespace Jasper.Testing.Samples
{
    public class ExceptionHandling
    {

    }

    // SAMPLE: AppWithErrorHandling
    public class AppWithErrorHandling : JasperRegistry
    {
        public AppWithErrorHandling()
        {
            // On a SqlException, reschedule the message to be retried
            // at 3 seconds, then 15, then 30 seconds later
            Handlers.Retries.Add(x => x.Handle<SqlException>()
                .Reschedule(3.Seconds(), 15.Seconds(), 30.Seconds()));


        }
    }
    // ENDSAMPLE
}
