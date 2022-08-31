using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Testing.Runtime;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class CompositeContinuationTests
    {
        [Fact]
        public async ValueTask executes_all_continuations()
        {
            var inner1 = Substitute.For<IContinuation>();
            var inner2 = Substitute.For<IContinuation>();

            var continuation = new CompositeContinuation(inner1, inner2);

            var context = Substitute.For<IMessageContext>();
            var runtime = new MockJasperRuntime();
            var now = DateTimeOffset.UtcNow;

            await continuation.ExecuteAsync(context, runtime, now);

            await inner1.Received().ExecuteAsync(context, runtime, now);
            await inner2.Received().ExecuteAsync(context, runtime, now);

        }

        [Fact]
        public async ValueTask executes_all_continuations_even_on_failures()
        {
            var inner1 = Substitute.For<IContinuation>();
            var inner2 = Substitute.For<IContinuation>();

            var continuation = new CompositeContinuation(inner1, inner2);

            var context = Substitute.For<IMessageContext>();
            var runtime = new MockJasperRuntime();
            var now = DateTimeOffset.UtcNow;

            inner1.ExecuteAsync(context, runtime, now).Throws(new DivideByZeroException());

            await continuation.ExecuteAsync(context, runtime, now);

            await inner2.Received().ExecuteAsync(context, runtime, now);

        }
    }
}
