using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime
{
    public class ReceiverContentTypeHandling
    {
        public ReceiverContentTypeHandling()
        {
            theGraph = new ChannelGraph();
            theNode = new ChannelNode(new Uri("memory://foo"));

            thePipeline = new RecordingHandlerPipeline();

            theCallback = Substitute.For<IMessageCallback>();

            theReceiver = new Receiver(thePipeline, theGraph, theNode);
        }

        private readonly ChannelGraph theGraph;
        private readonly ChannelNode theNode;
        private readonly RecordingHandlerPipeline thePipeline;
        private readonly Receiver theReceiver;
        private IMessageCallback theCallback;

        [Fact]
        public void if_no_content_type_is_specified_on_envelope_or_channel_use_graph_default()
        {
            theGraph.AcceptedContentTypes.Add("text/json");
            theNode.AcceptedContentTypes.Any().ShouldBeFalse();

            var headers = new Dictionary<string, string>();
            theReceiver.Receive(new byte[0], headers, Substitute.For<IMessageCallback>());

            headers[Envelope.ContentTypeKey].ShouldBe("text/json");
        }

        [Fact]
        public void if_no_content_type_is_specified_use_channel_default_when_it_exists()
        {
            theGraph.AcceptedContentTypes.Add("text/json");
            theNode.AcceptedContentTypes.Add("text/xml");

            var headers = new Dictionary<string, string>();
            theReceiver.Receive(new byte[0], headers, Substitute.For<IMessageCallback>());

            headers[Envelope.ContentTypeKey].ShouldBe("text/xml");
        }

        [Fact]
        public void the_envelope_content_type_wins()
        {
            theGraph.AcceptedContentTypes.Add("text/json");
            theNode.AcceptedContentTypes.Add("text/xml");


            var headers = new Dictionary<string, string>();
            headers[Envelope.ContentTypeKey] = "text/plain";
            theReceiver.Receive(new byte[0], headers, Substitute.For<IMessageCallback>());

            headers[Envelope.ContentTypeKey].ShouldBe("text/plain");
        }

        [Fact]
        public void should_set_the_received_at_envelope()
        {
            var headers = new Dictionary<string, string>();
            theReceiver.Receive(new byte[0], headers, Substitute.For<IMessageCallback>());

            thePipeline.Invoked.Single().ReceivedAt.ShouldBe(theNode.Uri);
        }

    }

    public class RecordingHandlerPipeline : IHandlerPipeline
    {
        public IList<Envelope> Invoked = new List<Envelope>();

        public IContinuation Continuation { get; set; }

        // just to satisfy the interface
        public Envelope Envelope { get; set; }

        public Task Invoke(Envelope envelope, ChannelNode receiver)
        {
            Invoked.Add(envelope);
            return Task.CompletedTask;
        }
    }

}