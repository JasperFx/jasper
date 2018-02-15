using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Testing.Messaging.Runtime;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class RequestReplyTimeoutTests
    {
        [Fact]
        public Task exceed_the_timeout()
        {
            var watcher = new ReplyWatcher();

            return Exception<TimeoutException>.ShouldBeThrownByAsync(() => watcher.StartWatch<Message1>(Guid.NewGuid(), 1.Seconds()));
        }
    }
}
