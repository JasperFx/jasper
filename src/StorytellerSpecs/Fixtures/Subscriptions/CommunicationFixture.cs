using System;
using System.Collections.Generic;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Tracking;
using Lamar;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class CommunicationFixture : BusFixture
    {
        private bool _initialized;
        private NodesCollection _nodes;
        private IJasperHost _publisher;

        public CommunicationFixture()
        {
            Title = "Communication via Subscriptions";
        }

        public override void SetUp()
        {
            _nodes = new NodesCollection();
            _publisher = null;
            _initialized = false;
        }

        public override void TearDown()
        {
            _nodes.Dispose();
        }

        [ExposeAsTable("The 'standin' Uri lookups are")]
        public void UriAliasesAre(Uri Alias, Uri Actual)
        {
            _nodes.Aliases[Alias] = Actual;
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
            return VerifySetOf(received).Titled("All the messages received should be")
                .MatchOn(x => x.ServiceName, x => x.MessageType, x => x.Name);
        }


        private IList<MessageRecord> received()
        {
            return _nodes.Tracker.Records;
        }


        [FormatAs("Send message {messageType} named {name}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType, string name)
        {
            if (!_initialized)
            {
                if (_publisher == null)
                {
                    var registry = new JasperRegistry
                    {
                        ServiceName = "Publisher"
                    };

                    _publisher = _nodes.Add(registry);
                }


                _initialized = true;
            }

            var history = _nodes.History;

            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            var waiter = history.Watch(() => { _publisher.Messaging.Send(message).Wait(); });

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
            _registry.Handlers.DisableConventionalDiscovery();

            _registry.Handlers.IncludeType<Message1Handler>();
            _registry.Handlers.IncludeType<Message2Handler>();
            _registry.Handlers.IncludeType<Message3Handler>();
            _registry.Handlers.IncludeType<Message4Handler>();
            _registry.Handlers.IncludeType<Message5Handler>();
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
    }

    public class NodesCollection : IDisposable
    {
        private readonly IList<IJasperHost> _hosts = new List<IJasperHost>();

        public readonly Dictionary<Uri, Uri> Aliases = new Dictionary<Uri, Uri>();
        public readonly MessageHistory History = new MessageHistory();

        public readonly MessageTracker Tracker = new MessageTracker();

        public void Dispose()
        {
            foreach (var runtime in _hosts) runtime.Dispose();
        }

        public IJasperHost Add(JasperRegistry registry)
        {
            registry.Services.For<MessageTracker>().Use(Tracker);
            registry.Services.For<MessageHistory>().Use(History);

            registry.Services.For<IMessageLogger>().Use<MessageTrackingLogger>().Singleton();

            var runtime = JasperHost.For(registry);
            _hosts.Add(runtime);

            return runtime;
        }
    }
}
