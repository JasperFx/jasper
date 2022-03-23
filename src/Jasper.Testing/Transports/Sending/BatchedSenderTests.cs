using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Transports.Sending
{
    public class BatchedSenderTests
    {
        public BatchedSenderTests()
        {
            theSender = new BatchedSender(TransportConstants.RepliesUri, theProtocol, theCancellation.Token,
                NullLogger.Instance);

            theSender.RegisterCallback(theSenderCallback);

            theBatch = new OutgoingMessageBatch(theSender.Destination, new[]
            {
                Envelope.ForPing(TransportConstants.LocalUri),
                Envelope.ForPing(TransportConstants.LocalUri),
                Envelope.ForPing(TransportConstants.LocalUri),
                Envelope.ForPing(TransportConstants.LocalUri),
                Envelope.ForPing(TransportConstants.LocalUri),
                Envelope.ForPing(TransportConstants.LocalUri)
            });

            theBatch.Messages.Each(x => x.Destination = theBatch.Destination);
        }

        private readonly ISenderProtocol theProtocol = Substitute.For<ISenderProtocol>();
        private readonly CancellationTokenSource theCancellation = new CancellationTokenSource();
        private readonly BatchedSender theSender;
        private readonly ISenderCallback theSenderCallback = Substitute.For<ISenderCallback>();
        private readonly OutgoingMessageBatch theBatch;

        [Fact]
        public async Task call_send_batch_if_not_latched_and_not_cancelled()
        {
            await theSender.SendBatch(theBatch);

#pragma warning disable 4014
            theProtocol.Received().SendBatch(theSenderCallback, theBatch);
#pragma warning restore 4014
        }

        [Fact]
        public async Task do_not_call_send_batch_if_cancelled()
        {
            theCancellation.Cancel();

            await theSender.SendBatch(theBatch);

#pragma warning disable 4014
            theProtocol.DidNotReceive().SendBatch(theSenderCallback, theBatch);
#pragma warning restore 4014
        }

        [Fact]
        public async Task do_not_call_send_batch_if_latched()
        {
            await theSender.LatchAndDrain();

            await theSender.SendBatch(theBatch);

#pragma warning disable 4014
            theProtocol.DidNotReceive().SendBatch(theSenderCallback, theBatch);

            theSenderCallback.Received().SenderIsLatched(theBatch);
#pragma warning restore 4014
        }
    }
}
