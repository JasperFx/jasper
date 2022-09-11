using System;
using System.Diagnostics;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Runtime;

public class when_creating_an_execution_activity
{
    private readonly Envelope theEnvelope;
    private readonly Activity theActivity;

    public when_creating_an_execution_activity()
    {
        theEnvelope = ObjectMother.Envelope();
        theEnvelope.ConversationId = Guid.NewGuid();

        theEnvelope.MessageType = "FooMessage";
        theEnvelope.CorrelationId = Guid.NewGuid().ToString();
        theEnvelope.Destination = new Uri("tcp://localhost:6666");

        theActivity = new Activity("process");
        theEnvelope.WriteTags(theActivity);
    }

    [Fact]
    public void should_set_the_otel_conversation_id_to_correlation_id()
    {
        theActivity.GetTagItem(JasperTracing.MessagingConversationId)
            .ShouldBe(theEnvelope.ConversationId);
    }

    [Fact]
    public void tags_the_message_id()
    {
        theActivity.GetTagItem(JasperTracing.MessagingMessageId)
            .ShouldBe(theEnvelope.Id);
    }

    [Fact]
    public void sets_the_message_system_to_destination_uri_scheme()
    {
        theActivity.GetTagItem(JasperTracing.MessagingSystem)
            .ShouldBe("tcp");
    }

    [Fact]
    public void sets_the_message_type_name()
    {
        theActivity.GetTagItem(JasperTracing.MessageType)
            .ShouldBe(theEnvelope.MessageType);
    }

    [Fact]
    public void the_destination_should_be_the_envelope_destination()
    {
        theActivity.GetTagItem(JasperTracing.MessagingDestination)
            .ShouldBe(theEnvelope.Destination);
    }

    [Fact]
    public void should_set_the_payload_size_bytes_when_it_exists()
    {
        theActivity.GetTagItem(JasperTracing.PayloadSizeBytes)
            .ShouldBe(theEnvelope.Data!.Length);
    }

    [Fact]
    public void trace_the_conversation_id()
    {
        theActivity.GetTagItem(JasperTracing.MessagingConversationId)
            .ShouldBe(theEnvelope.ConversationId);
    }
}
