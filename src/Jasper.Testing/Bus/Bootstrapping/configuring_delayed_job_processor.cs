using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class configuring_delayed_job_processor : BootstrappingContext
    {

        [Fact]
        public void default_is_to_use_in_memory_delayed_processor()
        {
            theRuntime.Container.DefaultSingletonIs<IScheduledJobProcessor, InMemoryScheduledJobProcessor>();
        }
    }
}
