using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Tracking;
using Jasper.TestSupport.Storyteller.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StoryTeller;
using StoryTeller.Engine;

namespace Jasper.TestSupport.Storyteller
{
    public static class JasperStorytellerHost
    {
        public static void Run<T>(string[] args) where T : JasperRegistry, new()
        {
            StorytellerAgent.Run(args, For<T>());
        }

        public static JasperStorytellerHost<T> For<T>() where T : JasperRegistry, new()
        {
            return new JasperStorytellerHost<T>(new T());
        }

        public static JasperStorytellerHost<JasperRegistry> Basic(Action<JasperRegistry> configure = null)
        {
            var jasperRegistry = new JasperRegistry();
            configure?.Invoke(jasperRegistry);

            return new JasperStorytellerHost<JasperRegistry>(jasperRegistry);
        }
    }

    public interface INodes
    {
        ExternalNode NodeFor(string serviceName);
    }

    public class JasperStorytellerHost<T> : ISystem, INodes where T : JasperRegistry
    {
        private readonly Dictionary<string, ExternalNode> _nodes = new Dictionary<string, ExternalNode>();

        public readonly CellHandling CellHandling = CellHandling.Basic();

        public readonly MessageHistory MessageHistory = new MessageHistory();

        private StorytellerMessageLogger _messageLogger;

        private IHost _host;
        private Task _warmup;


        public JasperStorytellerHost() : this(Activator.CreateInstance(typeof(T)).As<T>())
        {
        }

        public JasperStorytellerHost(T registry)
        {
            Registry = registry;

            Registry.Services.AddSingleton(MessageHistory);

            registry.Services.AddSingleton<INodes>(this);
            registry.Services.AddSingleton<MessageHistory>();

            registry.Services.ForSingletonOf<IMessageLogger>().Use<StorytellerMessageLogger>();
        }

        public T Registry { get; }

        public IHost Host
        {
            get
            {
                if (_host == null)
                    throw new InvalidOperationException(
                        "This property is not available until Storyteller either \"warms up\" the system or until the first specification is executed");

                return _host;
            }
        }

        public ExternalNode NodeFor(string serviceName)
        {
            if (!_nodes.ContainsKey(serviceName))
            {
                if (_nodes.Any())
                    throw new ArgumentOutOfRangeException(
                        $"Unknown node named '{serviceName}', the available node(s) are {_nodes.Keys.Select(x => "'" + x + "'").Join(", ")}");
                throw new ArgumentOutOfRangeException(
                    "There are no known external nodes for this JasperStorytellerHost");
            }

            return _nodes[serviceName];
        }

        public void Dispose()
        {
            if (_host != null)
            {
                afterAll();
                _host.Dispose();

                foreach (var node in _nodes.Values) node.Teardown();
            }
        }

        public CellHandling Start()
        {
            return CellHandling;
        }

        public IExecutionContext CreateContext()
        {
            beforeEach();
            return new JasperContext(this);
        }

        public Task Warmup()
        {
            _warmup = Task.Factory.StartNew(() =>
            {
                _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseJasper(Registry).Build();

                _messageLogger = _host.Services.GetService<IMessageLogger>().As<StorytellerMessageLogger>();
                _messageLogger.ServiceName = _host.Services.GetService<JasperOptions>().ServiceName;

                foreach (var node in _nodes.Values) node.Bootstrap(_messageLogger);

                beforeAll();
            });

            return _warmup;
        }

        public ExternalNode AddNode<TRegistry>(Action<TRegistry> configure = null) where TRegistry : JasperRegistry, new()
        {
            var registry = new TRegistry();
            configure?.Invoke(registry);

            return AddNode(registry);
        }

        public ExternalNode AddNode(JasperRegistry registry)
        {
            var node = new ExternalNode(registry);
            _nodes.Add(node.Host.Services.GetService<JasperOptions>().ServiceName, node);

            return node;
        }


        protected virtual void beforeAll()
        {
            // Nothing
        }

        protected virtual void afterEach(ISpecContext context)
        {
            // nothing
        }

        protected virtual void beforeEach()
        {
            // nothing
        }

        protected virtual void afterAll()
        {
            // nothing
        }


        public class JasperContext : IExecutionContext, IJasperContext
        {
            private readonly JasperStorytellerHost<T> _parent;

            public JasperContext(JasperStorytellerHost<T> parent)
            {
                _parent = parent;
            }

            void IDisposable.Dispose()
            {
            }

            public void BeforeExecution(ISpecContext context)
            {
                _parent._messageLogger.Start(context);
            }

            public void AfterExecution(ISpecContext context)
            {
                var reports = _parent._messageLogger.BuildReports();
                foreach (var report in reports) context.Reporting.Log(report);


                _parent.afterEach(context);
            }

            public TService GetService<TService>()
            {
                return _parent._host.Services.GetService<TService>();
            }

            public ExternalNode NodeFor(string nodeName)
            {
                return _parent.NodeFor(nodeName);
            }
        }
    }

    public interface IJasperContext
    {
        ExternalNode NodeFor(string nodeName);
    }

    public class ExternalNode
    {
        private readonly JasperRegistry _registry;

        public ExternalNode(JasperRegistry registry)
        {
            _registry = registry;
        }

        public IHost Host { get; private set; }

        internal void Bootstrap(IMessageLogger logger)
        {
            _registry.Services.AddSingleton(logger);
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseJasper(_registry).Start();
        }

        public Task Send<T>(T message)
        {
            return Host.Services.GetService<IMessageContext>().Send(message);
        }

        internal void Teardown()
        {
            Host?.Dispose();
        }
    }
}
