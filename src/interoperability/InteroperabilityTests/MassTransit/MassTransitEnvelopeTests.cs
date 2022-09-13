using System;
using Baseline.Dates;
using Jasper;
using Jasper.Runtime.Interop.MassTransit;
using Shouldly;
using Xunit;

namespace InteroperabilityTests.MassTransit;

public class MassTransitEnvelopeTests
{
    private readonly DateTimeOffset theSentTime = new DateTimeOffset(new DateTime(2022, 9, 13, 5, 0, 0));
    private readonly DateTimeOffset theExpirationTime = new DateTimeOffset(new DateTime(2022, 9, 13, 5, 5, 0));
    private readonly Envelope theEnvelope = new Envelope();
    private readonly MassTransitEnvelope theMassTransitEnvelope;

    public MassTransitEnvelopeTests()
    {
        theMassTransitEnvelope = new MassTransitEnvelope
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            ConversationId = Guid.NewGuid().ToString(),
            ExpirationTime = theExpirationTime.DateTime.ToUniversalTime(),
            SentTime = theSentTime.DateTime.ToUniversalTime()
        };

        theMassTransitEnvelope.Headers.Add("color", "purple");
        theMassTransitEnvelope.Headers.Add("number", 1);

        theMassTransitEnvelope.TransferData(theEnvelope);

        // TODO -- how to map the ResponseAddress to Jasper?

    }

    [Fact]
    public void map_headers()
    {
        theEnvelope.Headers["color"].ShouldBe("purple");
        theEnvelope.Headers["number"].ShouldBe("1");
    }

    [Fact]
    public void map_the_message_id()
    {
        theEnvelope.Id.ShouldBe(Guid.Parse(theMassTransitEnvelope.MessageId));
    }

    [Fact]
    public void map_the_correlation_id()
    {
        theEnvelope.CorrelationId.ShouldBe(theMassTransitEnvelope.CorrelationId);
    }

    [Fact]
    public void map_the_conversation_id()
    {
        theEnvelope.ConversationId.ShouldBe(Guid.Parse(theMassTransitEnvelope.ConversationId));
    }

    [Fact]
    public void map_the_expiration_time()
    {
        theEnvelope.DeliverBy.ShouldBe(theExpirationTime);
    }

    [Fact]
    public void map_the_sent_time()
    {
        theEnvelope.SentAt.ShouldBe(theSentTime);
    }
}
