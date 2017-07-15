using System;
using System.Collections.Generic;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Stubs;
using Xunit;

namespace Jasper.Testing.Bus.Runtime
{
    public class EnvelopeTester2
    {
        private Dictionary<string, string> theOriginalHeaders;
        private Lazy<Envelope> _envelope;
        private readonly Lazy<Dictionary<string, string>> _derivedHeaders;




        public EnvelopeTester2()
        {
            theOriginalHeaders = new Dictionary<string, string>();

            _envelope = new Lazy<Envelope>(() => new Envelope(new byte[] {1, 2, 3}, theOriginalHeaders, new StubMessageCallback()));

            _derivedHeaders = new Lazy<Dictionary<string, string>>(() => theEnvelope.WriteHeaders());
        }

        private Dictionary<string, string> theDerivedHeaders => _derivedHeaders.Value;
        private Envelope theEnvelope => _envelope.Value;

/*
        [Fact]
        public void use_correlation_id_from_headers()
        {
            throw new NotImplementedException("Do.");
        }

        [Fact]
        public void write_correlation_id_to_headers()
        {
            throw new NotImplementedException("Do.");
        }

        [Fact]
        public void assign_a_new_correlation_id_if_none_in_headers()
        {
            throw new NotImplementedException("Do.");
        }
*/





    }
}
