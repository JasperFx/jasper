using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Runtime.Handlers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class CircuitBreakerWrappedMessageHandlerTests
    {

        private readonly IMessageSuccessTracker theTracker = Substitute.For<IMessageSuccessTracker>();
        private readonly IMessageHandler theInnerHandler = Substitute.For<IMessageHandler>();
        private readonly CircuitBreakerWrappedMessageHandler theHandler;

        public CircuitBreakerWrappedMessageHandlerTests()
        {
            theHandler = new CircuitBreakerWrappedMessageHandler(theInnerHandler, theTracker);
        }

        [Fact]
        public async Task successful_execution()
        {
            var context = Substitute.For<IExecutionContext>();
            var token = CancellationToken.None;

            await theHandler.HandleAsync(context, token);

            await theInnerHandler.Received().HandleAsync(context, token);
            await theTracker.Received().TagSuccessAsync();
        }

        [Fact]
        public async Task failed_execution()
        {
            var context = Substitute.For<IExecutionContext>();
            var token = CancellationToken.None;

            var ex = new InvalidOperationException();

            theInnerHandler.HandleAsync(context, token).Throws(ex);

            Should.Throw<InvalidOperationException>(async () =>
            {
                await theHandler.HandleAsync(context, token);
            });

            await theTracker.Received().TagFailureAsync(ex);
        }
    }
}
