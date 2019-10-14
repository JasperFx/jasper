using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.ErrorHandling;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class RetryNowContinuationTester
    {
        [Fact]
        public async Task just_calls_through_to_the_context_pipeline_to_do_it_again()
        {
            var continuation = RetryNowContinuation.Instance;

            var context = Substitute.For<IMessageContext>();
            var advanced = Substitute.For<IAdvancedMessagingActions>();
            context.Advanced.Returns(advanced);

            var theEnvelope = ObjectMother.Envelope();
            await continuation.Execute(context, DateTime.UtcNow);

            await advanced.Received(1).Retry();
        }
    }
}
