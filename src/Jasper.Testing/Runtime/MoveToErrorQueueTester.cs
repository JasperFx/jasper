using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Jasper.Logging;
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

        }

        private readonly Exception theException = new DivideByZeroException();
        private readonly MoveToErrorQueue theContinuation;
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IChannelCallback theChannelCallback = Substitute.For<IChannelCallback>();


        [Fact]
        public async Task should_send_a_failure_ack()
        {
            var root = new MockMessagingRoot();
            await theContinuation.Execute(root, theChannelCallback, theEnvelope, null, DateTime.UtcNow);

            await root.Acknowledgements
                .Received()
                .SendFailureAcknowledgement(theEnvelope,$"Moved message {theEnvelope.Id} to the Error Queue.\n{theException}")
                ;
        }

        [Fact]
        public async Task if_not_supporting_native_dead_letter_queues()
        {
            var root = new MockMessagingRoot();
            await theContinuation.Execute(root, theChannelCallback, theEnvelope, null, DateTime.UtcNow);

            await root.Persistence.Received().MoveToDeadLetterStorage(theEnvelope, theException);
        }

        [Fact]
        public async Task logging_calls()
        {
            var root = new MockMessagingRoot();
            await theContinuation.Execute(root, theChannelCallback, theEnvelope, null, DateTime.UtcNow);

            root.MessageLogger.Received().MessageFailed(theEnvelope, theException);
            root.MessageLogger.Received().MovedToErrorQueue(theEnvelope, theException);
        }

        [Fact]
        public async Task use_native_error_queue_if_exists()
        {
            var callback = Substitute.For<IChannelCallback, IHasDeadLetterQueue>();
            var root = new MockMessagingRoot();

            await theContinuation.Execute(root, callback, theEnvelope, null, DateTime.UtcNow);

            await ((IHasDeadLetterQueue) callback).Received().MoveToErrors(theEnvelope, theException);

            await root.Persistence.DidNotReceive().MoveToDeadLetterStorage(theEnvelope, theException);
        }
    }
}
