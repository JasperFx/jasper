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
        private readonly IChannelCallback theChannelCallback = Substitute.For<IChannelCallback>();
        private readonly IExecutionContext theContext = Substitute.For<IExecutionContext>();

        [Fact]
        public async Task should_send_a_failure_ack()
        {
            var root = new MockMessagingRoot();
            await theContinuation.Execute(theChannelCallback, theContext, DateTime.UtcNow);

            await theContext
                .Received()
                .SendFailureAcknowledgement(theEnvelope,$"Moved message {theEnvelope.Id} to the Error Queue.\n{theException}")
                ;
        }

        [Fact]
        public async Task if_not_supporting_native_dead_letter_queues()
        {
            theContext.Persistence.Returns(Substitute.For<IEnvelopePersistence>());
            await theContinuation.Execute(theChannelCallback, theContext, DateTime.UtcNow);

            await theContext.Persistence.Received().MoveToDeadLetterStorage(theEnvelope, theException);
        }

        [Fact]
        public async Task logging_calls()
        {
            await theContinuation.Execute(theChannelCallback, theContext, DateTime.UtcNow);

            theContext.Logger.Received().MessageFailed(theEnvelope, theException);
            theContext.Logger.Received().MovedToErrorQueue(theEnvelope, theException);
        }

        [Fact]
        public async Task use_native_error_queue_if_exists()
        {
            var callback = Substitute.For<IChannelCallback, IHasDeadLetterQueue>();
            var root = new MockMessagingRoot();

            await theContinuation.Execute(callback, theContext, DateTime.UtcNow);

            await ((IHasDeadLetterQueue) callback).Received().MoveToErrors(theEnvelope, theException);

            await root.Persistence.DidNotReceive().MoveToDeadLetterStorage(theEnvelope, theException);
        }
    }
}
