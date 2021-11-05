using System.Collections.Generic;
using Jasper.Transports;
using Xunit;

namespace Jasper.Testing.Transports
{
    public class ProtocolTester
    {
        [Fact]
        public void can_map_simple_property()
        {

        }
    }

    public class DummyMessageProtocol : Protocol<DummyMessage>
    {
        protected override void writeOutgoingHeader(DummyMessage outgoing, string key, string value)
        {
            throw new System.NotImplementedException();
        }

        protected override bool tryReadIncomingHeader(DummyMessage incoming, string key, out string value)
        {
            return incoming.StringHeaders.TryGetValue(key, out value);
        }
    }

    public class DummyMessage
    {
        public string AppId { get; set; }

        public Dictionary<string, string> StringHeaders { get; } = new Dictionary<string, string>();
    }
}
