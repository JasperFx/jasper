using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Tracking;
using Jasper.Storyteller.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StoryTeller;
using StoryTeller.Engine;

namespace Jasper.Storyteller
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

        public readonly MessageHistory MessageHistory = new MessageHistory();

        private StorytellerMessageLogger _messageLogger;

        private JasperRuntime _runtime;

        public readonly CellHandling CellHandling = CellHandling.Basic();
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
            registry.Services.For<IMessageLogger>().DecorateAllWith<MessageTrackingLogger>();
            registry.Services.For<IMessageLogger>().DecorateAllWith<StorytellerMessageLogger>();

        }

        public ExternalNode AddNode<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return AddNode(registry);
        }

        public ExternalNode AddNode(JasperRegistry registry)
        {
            var node = new ExternalNode(registry);
            _nodes.Add(registry.ServiceName, node);

            return node;
        }

        public ExternalNode NodeFor(string serviceName)
        {
            if (!_nodes.ContainsKey(serviceName))
            {
                if (_nodes.Any())
                {
                    throw new ArgumentOutOfRangeException($"Unknown node named '{serviceName}', the available node(s) are {_nodes.Keys.Select(x => "'" + x + "'").Join(", ")}");
                }
                else
                {
                    throw new ArgumentOutOfRangeException("There are no known external nodes for this JasperStorytellerHost");
                }
            }

            return _nodes[serviceName];
        }

        public T Registry { get; }

        public JasperRuntime Runtime
        {
            get
            {
                if (_runtime == null)
                {
                    throw new InvalidOperationException(
                        "This property is not available until Storyteller either \"warms up\" the system or until the first specification is executed");
                }

                return _runtime;
            }
        }

        public void Dispose()
        {
            if (_runtime != null)
            {
                afterAll();
                _runtime.Dispose();

                foreach (var node in _nodes.Values)
                {
                    node.Teardown();
                }
            }
        }

        public CellHandling Start()
        {
            return CellHandling;
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

        public IExecutionContext CreateContext()
        {
            beforeEach();
            return new JasperContext(this);
        }

        public Task Warmup()
        {

            _warmup = Task.Factory.StartNew(() =>
            {
                _runtime = JasperRuntime.For(Registry);
                _messageLogger = _runtime.Get<IMessageLogger>().As<StorytellerMessageLogger>();
                _messageLogger.ServiceName = _runtime.ServiceName;

                _messageLogger = _runtime.Get<IMessageLogger>().As<StorytellerMessageLogger>();

                foreach (var node in _nodes.Values)
                {
                    node.Bootstrap(_messageLogger);
                }

                beforeAll();
            });

            return _warmup;
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

            public ExternalNode NodeFor(string nodeName)
            {
                return _parent.NodeFor(nodeName);
            }

            public void BeforeExecution(ISpecContext context)
            {
                _parent._messageLogger.Start(context);
            }

            public void AfterExecution(ISpecContext context)
            {
                var reports = _parent._messageLogger.BuildReports();
                foreach (var report in reports)
                {
                    context.Reporting.Log(report);
                }


                _parent.afterEach(context);
            }

            public TService GetService<TService>()
            {
                return _parent._runtime.Get<TService>();
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

        internal void Bootstrap(IMessageLogger logger)
        {
            _registry.Services.AddSingleton(logger);
            Runtime = JasperRuntime.For(_registry);
        }

        public JasperRuntime Runtime { get; private set; }

        public Task Send<T>(T message)
        {
            return Runtime.Get<IMessageContext>().Send(message);
        }

        internal void Teardown()
        {
            Runtime?.Dispose();
        }
    }
}
