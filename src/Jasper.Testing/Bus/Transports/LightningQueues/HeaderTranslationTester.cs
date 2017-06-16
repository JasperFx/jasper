using System;
using Jasper.Bus.Queues;
using Jasper.Bus.Transports.LightningQueues;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.LightningQueues
{
    public class HeaderTranslationTester
    {
        [Fact]
        public void translates_max_attempts()
        {
            var message = new OutgoingMessage();
            message.Headers.Add(LightningQueuesTransport.MaxAttemptsHeader, 1.ToString());
            message.TranslateHeaders();
            message.MaxAttempts.ShouldBe(1);
        }

        [Fact]
        public void translates_deliver_by()
        {
            var now = DateTime.Now;
            var message = new OutgoingMessage();
            message.Headers.Add(LightningQueuesTransport.DeliverByHeader, now.ToString("o"));
            message.TranslateHeaders();
            message.DeliverBy.ShouldBe(now);
        }

        [Fact]
        public void empty_when_headers_arent_present()
        {
            var message = new OutgoingMessage();
            message.TranslateHeaders();
            ShouldBeNullExtensions.ShouldBeNull(message.MaxAttempts);
            ShouldBeNullExtensions.ShouldBeNull(message.DeliverBy);
        }
    }
}