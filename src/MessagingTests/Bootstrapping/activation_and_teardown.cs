using System.Linq;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Shouldly;
using Xunit;

namespace MessagingTests.Bootstrapping
{
    public class activation_and_teardown : BootstrappingContext
    {
        [Fact]
        public void transport_is_disposed()
        {
            var runtime = theHost();
            var transport = runtime.Get<ITransport[]>().OfType<StubTransport>().Single();
            transport.WasDisposed.ShouldBeFalse();


            runtime.Dispose();

            transport.WasDisposed.ShouldBeTrue();
        }
    }
}
