using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.ErrorHandling;
using NSubstitute;
using Xunit;

namespace MessagingTests.ErrorHandling
{
    public class RequeueContinuationTester
    {
        [Fact]
        public async Task executing_just_puts_it_back_in_line_at_the_back_of_the_queue()
        {
            var envelope = ObjectMother.Envelope();

            var context = Substitute.For<IMessageContext>();
            context.Envelope.Returns(envelope);


            await RequeueContinuation.Instance.Execute(context, DateTime.Now);

            await envelope.Callback.Received(1).Requeue(envelope);
        }
    }
}
