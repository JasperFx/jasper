using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Bus.Runtime
{
    public class CompositeContinuationTester : InteractionContext<CompositeContinuation>
    {
        private readonly IContinuation[] inners;
        private readonly Envelope envelope = new Envelope();

        private readonly DateTime now = DateTime.Today.ToUniversalTime();

        public CompositeContinuationTester()
        {
            inners = Services.CreateMockArrayFor<IContinuation>(5);

            ClassUnderTest.Execute(envelope, MockFor<IEnvelopeContext>(), now);
        }

        [Fact]
        public void should_have_delegated_to_all_inners()
        {
            foreach (var continuation in inners)
            {
                continuation.Received(1).Execute(envelope, MockFor<IEnvelopeContext>(), now);
            }
        }
    }
}