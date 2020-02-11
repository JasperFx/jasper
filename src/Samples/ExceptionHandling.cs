using System;
using System.Data.SqlClient;
using Baseline.Dates;
using Jasper.ErrorHandling;
using Polly;

namespace Jasper.Testing.Samples
{
    public class ExceptionHandling
    {

    }

    // SAMPLE: AppWithErrorHandling
    public class AppWithErrorHandling : JasperOptions
    {
        public AppWithErrorHandling()
        {
            // On a SqlException, reschedule the message to be retried
            // at 3 seconds, then 15, then 30 seconds later
            Handlers.OnException<SqlException>()
                .RetryLater(3.Seconds(), 15.Seconds(), 30.Seconds());


        }
    }
    // ENDSAMPLE


    public class SqlException : Exception{}
}
