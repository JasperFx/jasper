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

            var context = Substitute.For<IExecutionContext>();
            context.Envelope.Returns(envelope);

            await continuation.Execute(context, DateTime.UtcNow);

            await context.Received(1).RetryExecutionNow();
        }
    }
}
