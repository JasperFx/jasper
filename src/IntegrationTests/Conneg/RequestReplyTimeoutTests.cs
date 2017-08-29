using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Runtime;
using Xunit;

namespace IntegrationTests.Conneg
{
    public class RequestReplyTimeoutTests
    {
        [Fact]
        public Task exceed_the_timeout()
        {
            var watcher = new ReplyWatcher();

            return Exception<TimeoutException>.ShouldBeThrownBy(() => watcher.StartWatch<Message1>(Guid.NewGuid().ToString(), 1.Seconds()));
        }
    }
}
