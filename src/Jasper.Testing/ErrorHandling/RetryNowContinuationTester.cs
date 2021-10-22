using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Testing.Runtime;
using Jasper.Transports;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class RetryNowContinuationTester
    {
        [Fact]
        public async Task just_calls_through_to_the_context_pipeline_to_do_it_again()
        {
            var continuation = RetryNowContinuation.Instance;

            var envelope = ObjectMother.Envelope();
            envelope.Attempts = 1;
            var channel = Substitute.For<IChannelCallback>();

            var root = new MockMessagingRoot();
            root.Pipeline.Invoke(envelope, channel).Returns(Task.CompletedTask);

            var context = Substitute.For<IExecutionContext>();
            context.Root.Returns(root);

            await continuation.Execute(channel, envelope, context, DateTime.UtcNow);

            await root.Pipeline.Received(1).Invoke(envelope, channel);
        }
    }
}
