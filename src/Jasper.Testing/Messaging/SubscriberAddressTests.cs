using Jasper.Messaging.Configuration;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class SubscriberAddressTests
    {
        [Fact]
        public void has_an_alias()
        {
            var fake = "fake://one".ToUri();
            var address = new SubscriberAddress(fake);
            var lookups = new UriAliasLookup(new IUriLookup[0]);

            var real = "real://one".ToUri();
            lookups.SetAlias(fake, real);

            address.ReadAlias(lookups);

            address.Uri.ShouldBe(real);
            address.Alias.ShouldBe(fake);
        }

        [Fact]
        public void does_not_have_an_alias()
        {
            var uri = "loopback://one".ToUri();
            var address = new SubscriberAddress(uri);
            var lookups = new UriAliasLookup(new IUriLookup[0]);


            address.ReadAlias(lookups);

            address.Uri.ShouldBe(uri);
            address.Alias.ShouldBeNull();
        }


    }
}
