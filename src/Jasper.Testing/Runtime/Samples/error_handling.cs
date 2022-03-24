using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Runtime.Handlers;
using Jasper.Transports;
using Lamar;
using LamarCodeGeneration;
using Polly;

namespace Jasper.Testing.Runtime.Samples
{
    public class error_handling
    {
    }

    #region sample_ErrorHandlingPolicy
    // This error policy will apply to all message types in the namespace
    // 'MyApp.Messages', and add a "requeue on SqlException" to all of these
    // message handlers
    public class ErrorHandlingPolicy : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, GenerationRules rules, IContainer container)
        {
            var matchingChains = graph
                .Chains
                .Where(x => x.MessageType.IsInNamespace("MyApp.Messages"));

            foreach (var chain in matchingChains)
            {
                chain.OnException<SqlException>().Requeue(2);
            }
        }
    }
    #endregion

    #region sample_MyApp_with_error_handling
    public class MyApp : JasperOptions
    {
        public MyApp()
        {
            Handlers.GlobalPolicy<ErrorHandlingPolicy>();
        }
    }
    #endregion

    #region sample_GlobalErrorHandlingConfiguration
    public class GlobalRetryApp : JasperOptions
    {
        public GlobalRetryApp()
        {
            Handlers.OnException<TimeoutException>().RetryLater(5.Seconds());
            Handlers.OnException<SecurityException>().MoveToErrorQueue();

            // You can also apply an additional filter on the
            // exception type for finer grained policies
            Handlers
                .OnException<SocketException>(ex => ex.Message.Contains("not responding"))
                .RetryLater(5.Seconds());
        }
    }
    #endregion


    #region sample_configure_error_handling_per_chain_with_configure
    public class MyErrorCausingHandler
    {
        // This method signature is meaningful
        public static void Configure(HandlerChain chain)
        {
            // Requeue on IOException for a maximum
            // of 3 attempts
            chain.OnException<IOException>()
                .Requeue(3);
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
    #endregion

    public class InvoiceCreated
    {
        public DateTime Time { get; set; }
        public string Purchaser { get; set; }
        public double Amount { get; set; }
    }

    public class InvoiceApproved
    {
    }

    #region sample_configuring_error_handling_with_attributes
    public class AttributeUsingHandler
    {
        [RetryLater(typeof(IOException), 5)]
        [RetryNow(typeof(SqlException))]
        [RequeueOn(typeof(InvalidOperationException))]
        [MoveToErrorQueueOn(typeof(DivideByZeroException))]
        [MaximumAttempts(2)]
        public void Handle(InvoiceCreated created)
        {
            // handle the invoice created message
        }
    }
    #endregion

    public class SqlException : Exception
    {
    }

    #region sample_filtering_by_exception_type
    public class FilteredApp : JasperOptions
    {
        public FilteredApp()
        {

            Handlers
                // You have all the available exception matching capabilities of Polly
                .OnException<SqlException>()
                .Or<InvalidOperationException>(ex => ex.Message.Contains("Intermittent message of some kind"))
                .OrInner<BadImageFormatException>()

                // And apply the "continuation" action to take if the filters match
                .Requeue();

            // Use different actions for different exception types
            Handlers.OnException<InvalidOperationException>().RetryNow();
        }
    }
    #endregion

    #region sample_continuation_actions
    public class ContinuationTypes : JasperOptions
    {
        public ContinuationTypes()
        {

            // Try to execute the message again without going
            // back through the queue with a maximum number of attempts
            // The default is 3
            // The message will be dead lettered if it exceeds the maximum
            // number of attemts
            Handlers.OnException<SqlException>().RetryNow(5);


            // Retry the message again, but wait for the specified time
            // The message will be dead lettered if it exhausts the delay
            // attempts
            Handlers
                .OnException<SqlException>()
                .RetryLater(3.Seconds(), 10.Seconds(), 20.Seconds());

            // Put the message back into the queue where it will be
            // attempted again
            // The message will be dead lettered if it exceeds the maximum number
            // of attempts
            Handlers.OnException<SqlException>().Requeue(5);

            // Immediately move the message into the error queue for this transport
            Handlers.OnException<SqlException>().MoveToErrorQueue();
        }
    }
    #endregion



    public class FailedOnSecurity
    {
        public FailedOnSecurity(string message)
        {
        }
    }

    #region sample_AppWithCustomContinuation
    public class AppWithCustomContinuation : JasperOptions
    {
        public AppWithCustomContinuation()
        {
            Handlers.OnException<UnauthorizedAccessException>()

                // The With() function takes a lambda factory for
                // custom IContinuation objects
                .With((envelope, exception) => new RaiseAlert(exception));
        }
    }
    #endregion

    #region sample_RaiseAlert_Continuation
    public class RaiseAlert : IContinuation
    {
        private readonly Exception _ex;

        public RaiseAlert(Exception ex)
        {
            _ex = ex;
        }

        public async Task Execute(IExecutionContext execution,
            DateTime utcNow)
        {
            // Raise a separate "alert" event message
            var session = execution.NewPublisher();
            await session.Schedule(execution.Envelope.Message, utcNow.AddHours(1));
            await session.SendAsync(new RescheduledAlert()
            {
                Id = execution.Envelope.Id,
                ExceptionText = _ex.ToString()

            });


        }
    }
    #endregion

    public class RescheduledAlert
    {
        public Guid Id { get; set; }
        public string ExceptionText { get; set; }
    }





}
