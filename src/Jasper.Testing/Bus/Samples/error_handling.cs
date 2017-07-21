using System;
using System.IO;
using System.Linq;
using System.Security;
using Baseline;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Testing.Bus.Samples
{
    public class error_handling
    {

    }

    // SAMPLE: ErrorHandlingPolicy
    public class ErrorHandlingPolicy : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph)
        {
            var matchingChains = graph.Chains.Where(x => x.MessageType.IsInNamespace("MyApp.Messages"));

            foreach (var chain in matchingChains)
            {
                chain.MaximumAttempts = 2;
                chain.OnException<SqlException>()
                    .Requeue();
            }
        }
    }
    // ENDSAMPLE

    // SAMPLE: MyApp-with-error-handling
    public class MyApp : JasperRegistry
    {
        public MyApp()
        {
            Policies.Global<ErrorHandlingPolicy>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: GlobalErrorHandlingConfiguration
    public class GlobalRetryApp : JasperRegistry
    {
        public GlobalRetryApp()
        {
            ErrorHandling
                .OnException<TimeoutException>()
                .RetryLater(5.Seconds());

            ErrorHandling
                .OnException<SecurityException>()
                .MoveToErrorQueue();
        }
    }
    // ENDSAMPLE


    // SAMPLE: configure-error-handling-per-chain-with-configure
    public class MyErrorCausingHandler
    {
        public static void Configure(HandlerChain chain)
        {
            chain.OnException<IOException>()
                .Requeue();

            chain.MaximumAttempts = 3;
        }


        public void Handle(InvoiceCreated created)
        {
            // handle the invoice created message
        }

        public void Handle(InvoiceApproved approved)
        {
            // handle the invoice approved message
        }
    }
    // ENDSAMPLE

    public class InvoiceCreated{}
    public class InvoiceApproved{}

    // SAMPLE: configuring-error-handling-with-attributes
    public class AttributeUsingHandler
    {
        [RetryLaterOn(typeof(IOException), 5)]
        [RetryOn(typeof(SqlException))]
        [RequeueOn(typeof(InvalidOperationException))]
        [MoveToErrorQueueOn(typeof(DivideByZeroException))]
        [MaximumAttempts(2)]
        public void Handle(InvoiceCreated created)
        {
            // handle the invoice created message
        }
    }
    // ENDSAMPLE

    public class SqlException : Exception{}

    // SAMPLE: filtering-by-exception-type
    public class FilteredApp : JasperRegistry
    {
        public FilteredApp()
        {
            ErrorHandling.OnException<SqlException>().Requeue();

            ErrorHandling.OnException(typeof(InvalidOperationException)).Retry();
        }
    }
    // ENDSAMPLE

    // SAMPLE: continuation-actions
    public class ContinuationTypes : JasperRegistry
    {
        public ContinuationTypes()
        {
            // Try to execute the message again without going
            // back through the queue
            ErrorHandling.OnException<SqlException>().Retry();

            // Retry the message again, but wait for the specified time
            ErrorHandling.OnException<SqlException>().RetryLater(3.Seconds());

            // Put the message back into the queue where it will be
            // attempted again
            ErrorHandling.OnException<SqlException>().Requeue();

            // Move the message into the error queue for this transport
            ErrorHandling.OnException<SqlException>().MoveToErrorQueue();
        }
    }
    // ENDSAMPLE


    // SAMPLE: RespondWithMessages
    public class RespondWithMessages : JasperRegistry
    {
        public RespondWithMessages()
        {
            ErrorHandling.OnException<SecurityException>()
                .RespondWithMessage((ex, envelope) =>
                {
                    return new FailedOnSecurity(ex.Message);
                });
        }
    }
    // ENDSAMPLE

    public class FailedOnSecurity
    {
        public FailedOnSecurity(string message)
        {
        }
    }


    // SAMPLE: CustomErrorHandler
    public class CustomErrorHandler : IErrorHandler
    {
        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            if (ex.Message.Contains("timed out"))
            {
                return new DelayedRetryContinuation(3.Seconds());
            }

            // If the handler doesn't apply to the exception,
            // return null to tell Jasper to try the next error handler (if any)
            return null;
        }
    }
    // ENDSAMPLE

    // SAMPLE: Registering-CustomErrorHandler
    public class CustomErrorHandlingApp : JasperRegistry
    {
        public CustomErrorHandlingApp()
        {
            // TODO -- make a nicer syntax for this
            ErrorHandling.ErrorHandlers.Add(new CustomErrorHandler());
        }
    }
    // ENDSAMPLE
}
