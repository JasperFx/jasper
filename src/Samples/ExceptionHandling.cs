using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.ErrorHandling;
using Microsoft.Extensions.Hosting;

namespace Samples
{
    public class ExceptionHandling
    {

    }



    public static class AppWithErrorHandling
    {
        public static async Task sample()
        {
            #region sample_AppWithErrorHandling
            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // On a SqlException, reschedule the message to be retried
                    // at 3 seconds, then 15, then 30 seconds later
                    opts.Handlers.OnException<SqlException>()
                        .RetryLater(3.Seconds(), 15.Seconds(), 30.Seconds());

                    // This is another equivalent option
                    opts.Handlers.OnException<TimeoutException>()
                        .TakeActions(x =>
                        {
                            x.RetryLater(3.Seconds());
                            x.RetryLater(15.Seconds());
                            x.RetryLater(30.Seconds());

                            // Jasper will automatically move the
                            // message to the dead letter queue
                            // after a 4th failure
                        });
                }).StartAsync();
            #endregion
        }

        public static async Task with_scripted_error_handling()
        {
            #region sample_AppWithScriptedErrorHandling

            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Handlers.OnException<TimeoutException>()
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
                }).StartAsync();

            #endregion
        }
    }



    public class SqlException : Exception{}
}
