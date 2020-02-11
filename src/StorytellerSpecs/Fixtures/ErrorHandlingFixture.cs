using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using StoryTeller;
using StoryTeller.Grammars.Tables;
using StorytellerSpecs.Stub;

namespace StorytellerSpecs.Fixtures
{
    [SelectionList("ErrorTypes")]
    public class ErrorType
    {
        public ErrorType(string errorType)
        {
            Type = ErrorCausingMessageHandler.GetExceptionType(errorType);
        }

        public Type Type { get; }
    }

    public abstract class ErrorHandlingFixtureBase : Fixture
    {
        protected ErrorHandlingFixtureBase()
        {
            AddSelectionValues("ErrorTypes",
                ErrorCausingMessageHandler.ExceptionTypes.Select(x => x.Name).Concat(new[] {"No Error"}).ToArray());
        }
    }

    [Hidden]
    public class GlobalErrorHandlingFixture : ErrorHandlingFixtureBase
    {
        protected IHasRetryPolicies _errorHandling;

        public override void SetUp()
        {
            _errorHandling = Context.State.Retrieve<IHasRetryPolicies>();
            _errorHandling.Retries.Clear();
        }

        [FormatAs("Retry on {errorType}")]
        public void RetryOn(ErrorType errorType)
        {
            _errorHandling.OnExceptionOfType(errorType.Type).RetryNow(3);
        }

        [FormatAs("Requeue on {errorType}")]
        public void RequeueOn(ErrorType errorType)
        {
            _errorHandling.OnExceptionOfType(errorType.Type).Requeue();
        }

        [FormatAs("Move to error queue on {errorType}")]
        public void MoveToErrorQueue(ErrorType errorType)
        {
            _errorHandling.OnExceptionOfType(errorType.Type).MoveToErrorQueue();
        }

        [FormatAs("Retry later in 5 seconds on {errorType}")]
        public void RetryLater(ErrorType errorType)
        {
            _errorHandling.OnExceptionOfType(errorType.Type).RetryLater(5.Seconds(), 5.Seconds(), 5.Seconds());
        }
    }

    [Hidden]
    public class ChainErrorHandlingFixture : GlobalErrorHandlingFixture
    {
        public override void SetUp()
        {
            base.SetUp();
            MaximumAttempts(1);
        }

        [FormatAs("Maximum Attempts for the Chain is {attempts}")]
        public void MaximumAttempts(int attempts)
        {
            _errorHandling.As<HandlerChain>().Retries.MaximumAttempts = attempts;
        }
    }


    public class ErrorHandlingFixture : ErrorHandlingFixtureBase
    {
        private IMessageContext _bus;
        private HandlerChain _chain;
        private HandlerGraph _graph;
        private IHost _host;
        private ErrorCausingMessage _message;
        private AttemptTracker _tracker;
        private StubTransport _transport;

        public override void SetUp()
        {
            _tracker = new AttemptTracker();

            var options = new JasperOptions();
            options.Extensions.UseMessageTrackingTestingSupport();
            options.Endpoints.As<TransportCollection>().Add(new StubTransport());

            options.Services.AddSingleton(_tracker);

            options.Endpoints.Publish(x => x.Message<ErrorCausingMessage>()
                .To("local://1".ToUri()));


            _host = JasperHost.For(options);

            _transport = _host.GetStubTransport();

            _graph = _host.Get<HandlerGraph>();
            _chain = _graph.ChainFor<ErrorCausingMessage>();


            _bus = _host.Get<IMessageContext>();
        }

        public override void TearDown()
        {
            _host.Dispose();

            _graph = null;
            _chain = null;
            _transport = null;
            _bus = null;
            _host = null;
        }

        public IGrammar IfTheGlobalHandlingIs()
        {
            return Embed<GlobalErrorHandlingFixture>("If the global error handling is")
                .Before(c => c.State.Store<IHasRetryPolicies>(_graph));
        }

        public IGrammar IfTheChainHandlingIs()
        {
            return Embed<ChainErrorHandlingFixture>("If the chain specific error handling is")
                .Before(c => c.State.Store<IHasRetryPolicies>(_chain));
        }

        [Hidden]
        public void MessageAttempt(int Attempt, [Header("Throws Error")] ErrorType errorType)
        {
            _message.Errors.SmartAdd(Attempt, errorType.Type.Name);
        }

        public IGrammar MessageAttempts()
        {
            return this[nameof(MessageAttempt)]
                .AsTable("If the message processing is")
                .Before(c => _message = new ErrorCausingMessage())
                .After(async c => await _bus.Send(_message));
        }


        [FormatAs("Send the message with no errors")]
        public async Task SendMessageWithNoErrors()
        {
            _message = new ErrorCausingMessage();
            await _bus.Send(_message);
        }

        [FormatAs("The message should have ended as '{result}' on attempt {attempt}")]
        public void MessageResult(
            out int attempt,
            [SelectionValues("Succeeded", "MovedToErrorQueue", "Retry in 5 seconds")]
            out string result)
        {
            attempt = _tracker.LastAttempt;
            result = "Unknown";

            var callback = _transport.LastCallback();
            if (callback == null) throw new Exception("Something went really wrong, there's no message history");

            if (callback.MarkedSucessful)
                result = "Succeeded";
            else if (callback.WasMovedToErrors)
                result = "MovedToErrorQueue";
            else if (callback.DelayedTo != null) result = "Retry in 5 seconds";
        }
    }

    public class AttemptTracker
    {
        public int LastAttempt;
    }

    public class ErrorCausingMessage
    {
        public Dictionary<int, string> Errors = new Dictionary<int, string>();
        public bool WasProcessed { get; set; }
        public int LastAttempt { get; set; }
    }

    public class ErrorCausingMessageHandler
    {
        public static readonly IList<Type> ExceptionTypes = new List<Type>
        {
            typeof(DivideByZeroException),
            typeof(DataMisalignedException),
            typeof(InvalidOperationException),
            typeof(ArgumentNullException)
        };


        public void Handle(ErrorCausingMessage message, Envelope envelope, AttemptTracker tracker)
        {
            tracker.LastAttempt = envelope.Attempts;

            if (!message.Errors.ContainsKey(envelope.Attempts))
            {
                message.WasProcessed = true;

                return;
            }

            var type = GetExceptionType(message.Errors[envelope.Attempts]);
            var ex = Activator.CreateInstance(type).As<Exception>();

            throw ex;
        }

        [JasperIgnore]
        public static Type GetExceptionType(string messageErrorType)
        {
            var type = ExceptionTypes.FirstOrDefault(x => x.Name == messageErrorType);
            return type;
        }
    }
}
