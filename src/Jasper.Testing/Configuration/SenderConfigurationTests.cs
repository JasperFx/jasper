using System;
using Jasper.Configuration;
using Jasper.Testing.Messaging;
using Jasper.Transports.Local;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class SenderConfigurationTests
    {
        [Fact]
        public void durably()
        {
            var endpoint = new LocalQueueSettings("foo");
            endpoint.Mode.ShouldBe(EndpointMode.BufferedInMemory);

            var expression = new SubscriberConfiguration(endpoint);
            expression.Durably();

            endpoint.Mode.ShouldBe(EndpointMode.Durable);
        }

        [Fact]
        public void buffered_in_memory()
        {
            var endpoint = new LocalQueueSettings("foo");
            endpoint.Mode = EndpointMode.Durable;

            var expression = new SubscriberConfiguration(endpoint);
            expression.BufferedInMemory();

            endpoint.Mode.ShouldBe(EndpointMode.BufferedInMemory);
        }

        [Fact]
        public void inline()
        {
            var endpoint = new LocalQueueSettings("foo");
            endpoint.Mode = EndpointMode.Durable;

            var expression = new SubscriberConfiguration(endpoint);
            expression.SendInline();

            endpoint.Mode.ShouldBe(EndpointMode.Inline);
        }



        [Fact]
        public void customize_envelope_rules()
        {
            var endpoint = new LocalQueueSettings("foo");
            var expression = new SubscriberConfiguration(endpoint);
            expression.CustomizeOutgoing(e => e.Headers.Add("a", "one"));

            var envelope = ObjectMother.Envelope();

            endpoint.Customize(envelope);

            envelope.Headers["a"].ShouldBe("one");
        }

        public class OtherMessage
        {

        }

        public abstract class BaseMessage
        {

        }

        public class ExtendedMessage : BaseMessage{}


        [Fact]
        public void customize_per_specific_message_type()
        {
            var endpoint = new LocalQueueSettings("foo");
            var expression = new SubscriberConfiguration(endpoint);
            expression.CustomizeOutgoingMessagesOfType<OtherMessage>(e => e.Headers.Add("g", "good"));

            // Negative Case
            var envelope1 = new Envelope(new Message1());
            endpoint.Customize(envelope1);

            envelope1.Headers.ContainsKey("g").ShouldBeFalse();


            // Positive Case
            var envelope2 = new Envelope(new OtherMessage());
            endpoint.Customize(envelope2);

            envelope2.Headers["g"].ShouldBe("good");

        }


        [Fact]
        public void customize_per_specific_message_type_parent()
        {
            var endpoint = new LocalQueueSettings("foo");
            var expression = new SubscriberConfiguration(endpoint);
            expression.CustomizeOutgoingMessagesOfType<BaseMessage>(e => e.Headers.Add("g", "good"));

            // Negative Case
            var envelope1 = new Envelope(new Message1());
            endpoint.Customize(envelope1);

            envelope1.Headers.ContainsKey("g").ShouldBeFalse();


            // Positive Case
            var envelope2 = new Envelope(new ExtendedMessage());
            endpoint.Customize(envelope2);

            envelope2.Headers["g"].ShouldBe("good");

        }

        public class ColorMessage
        {
            public string Color { get; set; }
        }


    }
}
