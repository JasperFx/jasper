using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Tracking;
using Lamar;
using Microsoft.Extensions.Hosting;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class CommunicationFixture : BusFixture
    {
        private bool _initialized;
        private NodesCollection _nodes;
        private IHost _publisher;

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
                    var registry = c.State.Retrieve<JasperOptions>();
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
                if (_publisher == null)
                {
                    var registry = new JasperOptions
                    {
                        ServiceName = "Publisher",

                    };

                    registry.Extensions.UseMessageTrackingTestingSupport();

                    _publisher = _nodes.Add(registry);
                }


                _initialized = true;
            }


            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            await _publisher.SendMessageAndWait(message);

        }
    }


    [Hidden]
    public class NodeFixture : BusFixture
    {
        private JasperOptions _options;

        public NodeFixture()
        {
            Title = "Service Definition";
        }

        public override void SetUp()
        {
            _options = new JasperOptions();
            _options.Handlers.DisableConventionalDiscovery();

            _options.Handlers.IncludeType<Message1Handler>();
            _options.Handlers.IncludeType<Message2Handler>();
            _options.Handlers.IncludeType<Message3Handler>();
            _options.Handlers.IncludeType<Message4Handler>();
            _options.Handlers.IncludeType<Message5Handler>();
        }

        public override void TearDown()
        {
            Context.State.Store(_options);
        }

        [FormatAs("The service name is {serviceName}")]
        public void ForService(string serviceName)
        {
            _options.ServiceName = serviceName;
        }
    }

    public class NodesCollection : IDisposable
    {
        private readonly IList<IHost> _hosts = new List<IHost>();

        public readonly Dictionary<Uri, Uri> Aliases = new Dictionary<Uri, Uri>();

        public readonly MessageTracker Tracker = new MessageTracker();

        public void Dispose()
        {
            foreach (var runtime in _hosts) runtime.Dispose();
        }

        public IHost Add(JasperOptions options)
        {
            options.Services.For<MessageTracker>().Use(Tracker);

            options.Services.For<IMessageLogger>().Use<MessageTrackingLogger>().Singleton();

            var runtime = JasperHost.For(options);
            _hosts.Add(runtime);

            return runtime;
        }
    }
}
