using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Tracking;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class CommunicationFixture : BusFixture
    {
        private NodesCollection _nodes;
        private bool _initialized = false;
        private JasperRuntime _publisher;

        public CommunicationFixture()
        {
            Title = "Communication via Subscriptions";
        }

        public override void SetUp()
        {
            _nodes = new NodesCollection();
        }

        public override void TearDown()
        {
            _nodes.Dispose();
        }

        public IGrammar ForService()
        {
            return Embed<NodeFixture>("For a service")
                .After(c =>
                {
                    var registry = c.State.Retrieve<JasperRegistry>();
                    _nodes.Add(registry);
                    _initialized = false;
                });

        }

        public IGrammar TheMessagesSentShouldBe()
        {
            return VerifySetOf(sent).Titled("All the messages sent should be")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.Name);
        }


        private IList<MessageRecord> sent()
        {
            return _nodes.Tracker.Records;
        }


        [FormatAs("Send message {messageType} named {name}")]
        public async Task SendMessage([SelectionList("MessageTypes")] string messageType, string name)
        {
            if (!_initialized)
            {
                await _nodes.StoreSubscriptions();
                if (_publisher == null)
                {
                    var registry = new JasperRegistry
                    {
                        ServiceName = "Publisher"
                    };

                    _publisher = _nodes.Add(registry);
                }

                _publisher.ResetSubscriptions();
            }

            var history = _nodes.History;

            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            var waiter = history.Watch(() =>
            {
                _publisher.Bus.Send(message).Wait();
            });

            waiter.Wait(5.Seconds());

            StoryTellerAssert.Fail(!waiter.IsCompleted, "Messages were never completely tracked");
        }


    }

    [Hidden]
    public class NodeFixture : BusFixture
    {
        private JasperRegistry _registry;

        public NodeFixture()
        {
            Title = "Service Definition";
        }

        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _registry.Messaging.Handlers.ConventionalDiscoveryDisabled = true;
        }

        public override void TearDown()
        {
            Context.State.Store(_registry);
        }

        [FormatAs("The service name is {serviceName}")]
        public void ForService(string serviceName)
        {
            _registry.ServiceName = serviceName;
        }

        [FormatAs("Handles and subscibes to message {messageType} at port {port}")]
        public void SubscribesTo([SelectionList("MessageTypes")] string messageType, int port)
        {
            var uri = $"jasper://localhost:{port}/local".ToUri();
            _registry.Channels.ListenForMessagesFrom(uri);

            var type = messageTypeFor(messageType);

            _registry.Subscriptions.To(type).At(uri);

            var handlerType = typeof(MessageHandler<>).MakeGenericType(type);
            _registry.Messaging.Handlers.IncludeType(handlerType);
        }


    }

    public class NodesCollection : IDisposable
    {
        public readonly MessageTracker Tracker = new MessageTracker();
        public readonly MessageHistory History = new MessageHistory();
        public readonly InMemorySubscriptionsRepository Subscriptions = new InMemorySubscriptionsRepository();

        private readonly IList<JasperRuntime> _runtimes = new List<JasperRuntime>();


        public JasperRuntime Add(JasperRegistry registry)
        {
            registry.Services.For<MessageTracker>().Use(Tracker);
            registry.Services.For<MessageHistory>().Use(History);

            registry.Services.AddService<IBusLogger, MessageTrackingLogger>();

            var runtime = JasperRuntime.For(registry);
            _runtimes.Add(runtime);

            return runtime;
        }

        public async Task StoreSubscriptions()
        {
            foreach (var runtime in _runtimes)
            {
                await runtime.PersistSubscriptions();
            }
        }

        public void Dispose()
        {
            Subscriptions?.Dispose();
            foreach (var runtime in _runtimes)
            {
                runtime.Dispose();
            }
        }
    }
}
