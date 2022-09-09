using System.Diagnostics;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Runtime;

public class CommandBusTests
{

    [Fact]
    public void use_current_activity_root_id_as_correlation_id_if_exists()
    {
        var activity = new Activity("process");
        activity?.Start();

        try
        {
            var bus = new CommandBus(new MockJasperRuntime());
            bus.CorrelationId.ShouldBe(activity.RootId);
        }
        finally
        {
            activity?.Stop();
        }
    }
}
