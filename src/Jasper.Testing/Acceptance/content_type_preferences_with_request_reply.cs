using System;
using System.Linq;
using Baseline;
using Jasper.Runtime;
using Jasper.Serialization;
using Jasper.Util;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class content_type_preferences_with_request_reply : IntegrationContext
    {
        public content_type_preferences_with_request_reply(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void envelope_has_accepts_for_known_response_readers()
        {
            var envelope = Publisher.As<ExecutionContext>().EnvelopeForRequestResponse<Message1>(new Message2());

            envelope.AcceptedContentTypes.Last().ShouldBe(EnvelopeConstants.JsonContentType);
        }
    }

}
