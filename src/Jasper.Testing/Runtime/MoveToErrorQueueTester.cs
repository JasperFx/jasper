using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Transports;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class MoveToErrorQueueTester
    {
        public MoveToErrorQueueTester()
        {
            theContinuation = new MoveToErrorQueue(theException);
            theContext.Envelope.Returns(theEnvelope);
        }

        private readonly Exception theException = new DivideByZeroException();
        private readonly MoveToErrorQueue theContinuation;
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IMessageContext theContext = Substitute.For<IMessageContext>();
        private readonly MockJasperRuntime theRuntime = new MockJasperRuntime();

        [Fact]
        public async Task should_send_a_failure_ack()
        {

            await theContinuation.ExecuteAsync(theContext, theRuntime, DateTimeOffset.Now);

            await theContext
                .Received()
                .SendFailureAcknowledgementAsync($"Moved message {theEnvelope.Id} to the Error Queue.\n{theException}")
                ;
        }

        [Fact]
        public async Task logging_calls()
        {
            await theContinuation.ExecuteAsync(theContext, theRuntime, DateTimeOffset.Now);

            theContext.Logger.Received().MessageFailed(theEnvelope, theException);
            theContext.Logger.Received().MovedToErrorQueue(theEnvelope, theException);
        }

    }
}
