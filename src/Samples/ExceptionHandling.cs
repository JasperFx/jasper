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

            // This is another equivalent option
            Handlers.OnException<TimeoutException>()
                .TakeActions(x =>
                {
                    x.RetryLater(3.Seconds());
                    x.RetryLater(15.Seconds());
                    x.RetryLater(30.Seconds());

                    // Jasper will automatically move the
                    // message to the dead letter queue
                    // after a 4th failure
                });
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppWithScriptedErrorHandling
    public class AppWithScriptedErrorHandling : JasperOptions
    {
        public AppWithScriptedErrorHandling()
        {
            Handlers.OnException<TimeoutException>()
                .TakeActions(x =>
                {
                    // Just retry the message again on the
                    // first failure
                    x.RetryNow();

                    // On the 2nd failure, put the message back into the
                    // incoming queue to be retried later
                    x.Requeue();

                    // On the 3rd failure, retry the message again after a configurable
                    // cool-off period. This schedules the message
                    x.RetryLater(15.Seconds());
                    
                    // On the 4th failure, move the message to the dead letter queue
                    x.MoveToErrorQueue();

                    // Or instead you could just discard the message
                    // x.Discard();
                });
        }
    }
    // ENDSAMPLE


    public class SqlException : Exception{}
}
