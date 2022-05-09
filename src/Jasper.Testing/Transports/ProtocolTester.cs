using System;
using System.Collections.Generic;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
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

    public class DummyMessageProtocol : TransportEndpoint<DummyMessage>
    {
        protected override void writeOutgoingHeader(DummyMessage outgoing, string key, string value)
        {
            throw new System.NotImplementedException();
        }

        protected override bool tryReadIncomingHeader(DummyMessage incoming, string key, out string? value)
        {
            return incoming.StringHeaders.TryGetValue(key, out value);
        }

        public override Uri Uri { get; }
        public override Uri CorrectedUriForReplies()
        {
            throw new NotImplementedException();
        }

        public override void Parse(Uri? uri)
        {
            throw new NotImplementedException();
        }

        public override void StartListening(IJasperRuntime runtime)
        {
            throw new NotImplementedException();
        }

        protected override ISender CreateSender(IJasperRuntime root)
        {
            throw new NotImplementedException();
        }
    }

    public class DummyMessage
    {
        public string AppId { get; set; }

        public Dictionary<string, string> StringHeaders { get; } = new Dictionary<string, string>();
    }
}
