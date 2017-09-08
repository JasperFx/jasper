using System;
using System.Collections.Generic;
using System.Linq;
using Baseline.Dates;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Tracking;
using Jasper.Bus.Transports;
using Jasper.LightningDb;
using StoryTeller;
using StoryTeller.Results;
using StoryTeller.Util;
using Envelope = Jasper.Bus.Runtime.Envelope;

namespace StorytellerSpecs.Fixtures
{
    // TODO -- move this to the new Jasper.Storyteller when it exists
    public class StorytellerBusLogger : IBusLogger
    {
        private readonly ISpecContext _context;

        public StorytellerBusLogger(ISpecContext context)
        {
            _context = context;
        }

        public void Sent(Envelope envelope)
        {
            trace($"Sent {envelope}");
        }

        public void Received(Envelope envelope)
        {
            trace($"Received {envelope}");
        }

        public void ExecutionStarted(Envelope envelope)
        {

        }

        public void ExecutionFinished(Envelope envelope)
        {

        }

        public void MessageSucceeded(Envelope envelope)
        {
            trace($"Message {envelope} succeeded");
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            trace($"Message {envelope} failed");
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
            _context.Reporting.ReporterFor<BusErrors>().Exceptions.Add(ex);
        }

        public void NoHandlerFor(Envelope envelope)
        {
            trace($"No handler for {envelope}");
        }

        public void NoRoutesFor(Envelope envelope)
        {
            trace($"No routing for {envelope}");
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            trace($"Subscription mismatch: {mismatch}");
        }

        public void Undeliverable(Envelope envelope)
        {
            trace($"Envelope {envelope} cannot be delivered");
        }

        private void trace(string message)
        {
            _context.Reporting.ReporterFor<BusActivity>().Messages.Add(message);
        }
    }

    public class BusErrors : Report
    {
        public readonly IList<Exception> Exceptions = new List<Exception>();

        public string ToHtml()
        {
            var div = new HtmlTag("div");

            foreach (var exception in Exceptions)
            {
                div.Add("div").AddClasses("alert", "alert-warning").Text(exception.ToString());
            }


            return div.ToString();
        }

        public string Title { get; } = "Logged Bus Errors";
        public string ShortTitle { get; } = "Bus Errors";
        public int Count => Exceptions.Count;
    }

    public class BusActivity : Report
    {
        public readonly IList<string> Messages = new List<string>();

        public string ToHtml()
        {
            var ul = new HtmlTag("ul");

            foreach (var message in Messages)
            {
                ul.Add("li").Text(message);
            }


            return ul.ToString();
        }

        public string Title { get; } = "Bus Activity";
        public string ShortTitle { get; } = "Bus Activity";
        public int Count => Messages.Count;
    }

    [Hidden]
    public class ServiceBusApplication : BusFixture
    {
        private JasperRegistry _registry;
        private bool _waitForSubscriptions;

        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _waitForSubscriptions = false;

            _registry.Services.AddService<ITransport, StubTransport>();
            _registry.Services.ForConcreteType<MessageTracker>().Configure.Singleton();

            _registry.Services.ForConcreteType<MessageHistory>().Configure.Singleton();
            _registry.Services.AddService<IBusLogger, MessageTrackingLogger>();

            _registry.Services.For<LightningDbSettings>().Use(new LightningDbSettings
            {
                MaxDatabases = 20
            });

            _registry.Logging.LogBusEventsWith(new StorytellerBusLogger(Context));
        }

        public override void TearDown()
        {
            var runtime = JasperRuntime.For(_registry);
            var history = runtime.Container.GetInstance<MessageHistory>();
            var graph = runtime.Container.GetInstance<HandlerGraph>();


            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);
            _registry.Messaging.SendMatching(type.Name, t => t == type).To(channel);

            // Just makes the test harness listen for things
            _registry.Channels.ListenForMessagesFrom(channel);
        }

        [FormatAs("When a Message1 is received, it cascades a matching Message2")]
        public void ReceivingMessage1CascadesMessage2()
        {
            _registry.Handlers.IncludeType<Cascader1>();
        }

        [FormatAs("When Message2 is received, it cascades matching Message3 and Message4")]
        public void ReceivingMessage2CascadesMultiples()
        {
            _registry.Handlers.IncludeType<Cascader2>();
        }

        [FormatAs("Listen for incoming messages from {channel}")]
        public void ListenForMessagesFrom([SelectionList("Channels")] Uri channel)
        {
            _registry.Channels.ListenForMessagesFrom(channel);
        }

    }
}
