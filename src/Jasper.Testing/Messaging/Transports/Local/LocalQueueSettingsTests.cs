using System;
using Jasper.Messaging.Transports.Local;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Local
{
    public class LocalQueueSettingsTests
    {
        [Fact]
        public void should_set_the_Uri_in_constructor()
        {
            var endpoint = new LocalQueueSettings("foo");
            endpoint.Uri.ShouldBe(new Uri("local://foo"));
        }

        [Fact]
        public void create_by_uri()
        {
            var endpoint = new LocalQueueSettings(new Uri("local://foo"));
            endpoint.IsDurable.ShouldBeFalse();
            endpoint.Name.ShouldBe("foo");
        }

        [Fact]
        public void create_by_uri_case_insensitive()
        {
            var endpoint = new LocalQueueSettings(new Uri("local://Foo"));
            endpoint.IsDurable.ShouldBeFalse();
            endpoint.Name.ShouldBe("foo");
        }

        [Fact]
        public void create_by_uri_durable()
        {
            var endpoint = new LocalQueueSettings(new Uri("local://durable/foo"));
            endpoint.IsDurable.ShouldBeTrue();
            endpoint.Name.ShouldBe("foo");
        }
    }
}
