using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Tracking;
using Jasper.Util;
using StoryTeller;
using StoryTeller.Grammars.Tables;

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

                await _nodes.StoreSubscriptions();

                _publisher.ResetSubscriptions();

                _initialized = true;
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
            _registry.Handlers.DisableConventionalDiscovery(true);

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

        [FormatAs("Handles and subscibes to message {messageType} at port {port}")]
        public void SubscribesTo([SelectionList("MessageTypes")] string messageType, int port)
        {
            var uri = $"tcp://localhost:{port}/local".ToUri();
            SubscribeAtUri(messageType, uri);
        }

        [FormatAs("Handles and subscibes to message {messageType} at Uri {uri}")]
        public void SubscribeAtUri(string messageType, Uri uri)
        {
            _registry.Transports.ListenForMessagesFrom(uri);

            var type = messageTypeFor(messageType);

            _registry.Subscribe.To(type).At(uri);
        }
    }

    public class NodesCollection : IDisposable, IUriLookup
    {
        public readonly MessageTracker Tracker = new MessageTracker();
        public readonly MessageHistory History = new MessageHistory();
        public readonly InMemorySubscriptionsRepository Subscriptions = new InMemorySubscriptionsRepository();

        private readonly IList<JasperRuntime> _runtimes = new List<JasperRuntime>();



        public JasperRuntime Add(JasperRegistry registry)
        {
            registry.Services.For<MessageTracker>().Use(Tracker);
            registry.Services.For<MessageHistory>().Use(History);
            registry.Services.For<ISubscriptionsRepository>().Use(Subscriptions);

            registry.Services.AddTransient<IBusLogger, MessageTrackingLogger>();
            registry.Services.For<IUriLookup>().Add(this);

            var runtime = JasperRuntime.For(registry);
            _runtimes.Add(runtime);

            return runtime;
        }

        public string Protocol { get; } = "standin";

        public readonly Dictionary<Uri, Uri> Aliases = new Dictionary<Uri, Uri>();

        public Task<Uri[]> Lookup(Uri[] originals)
        {
            var actuals = originals.Select(x => Aliases[x]);
            return Task.FromResult(actuals.ToArray());
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
