using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Testing.Runtime;
using Jasper.Transports;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class RequeueContinuationTester
    {
        [Fact]
        public async Task executing_just_puts_it_back_in_line_at_the_back_of_the_queue()
        {

            var envelope = ObjectMother.Envelope();

            var context = Substitute.For<IMessageContext>();
            context.Envelope.Returns(envelope);


            await RequeueContinuation.Instance.ExecuteAsync(context, new MockJasperRuntime(), DateTime.Now);

            await context.Received(1).DeferAsync();
        }
    }
}
