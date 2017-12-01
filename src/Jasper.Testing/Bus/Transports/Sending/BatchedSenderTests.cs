using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.Sending
{
    public class BatchedSenderTests
    {
        private readonly ISenderProtocol theProtocol = Substitute.For<ISenderProtocol>();
        private readonly CancellationTokenSource theCancellation = new CancellationTokenSource();
        private BatchedSender theSender;
        private ISenderCallback theSenderCallback = Substitute.For<ISenderCallback>();
        private OutgoingMessageBatch theBatch;

        public BatchedSenderTests()
        {
            theSender = new BatchedSender(TransportConstants.RepliesUri, theProtocol, theCancellation.Token, CompositeTransportLogger.Empty());
            theSender.Start(theSenderCallback);

            theBatch = new OutgoingMessageBatch(theSender.Destination, new Envelope[]
            {
                Envelope.ForPing(),
                Envelope.ForPing(),
                Envelope.ForPing(),
                Envelope.ForPing(),
                Envelope.ForPing(),
                Envelope.ForPing()
            });

            theBatch.Messages.Each(x => x.Destination = theBatch.Destination);
        }

        [Fact]
        public async Task call_send_batch_if_not_latched_and_not_cancelled()
        {
            await theSender.SendBatch(theBatch);

            theProtocol.Received().SendBatch(theSenderCallback, theBatch);
        }

        [Fact]
        public async Task do_not_call_send_batch_if_cancelled()
        {
            theCancellation.Cancel();

            await theSender.SendBatch(theBatch);

            theProtocol.DidNotReceive().SendBatch(theSenderCallback, theBatch);
        }

        [Fact]
        public async Task do_not_call_send_batch_if_latched()
        {
            theSender.LatchAndDrain();

            await theSender.SendBatch(theBatch);

            theProtocol.DidNotReceive().SendBatch(theSenderCallback, theBatch);

            theSenderCallback.Received().SenderIsLatched(theBatch);
        }
    }
}
