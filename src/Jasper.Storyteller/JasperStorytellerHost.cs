using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http;
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
        public static void Run<T>(string[] args) where T : JasperHttpRegistry, new()
        {
            StorytellerAgent.Run(args, For<T>());
        }

        public static JasperStorytellerHost<T> For<T>() where T : JasperHttpRegistry, new()
        {
            return new JasperStorytellerHost<T>(new T());
        }

        public static JasperStorytellerHost<JasperHttpRegistry> Basic(Action<JasperHttpRegistry> configure = null)
        {
            var jasperRegistry = new JasperHttpRegistry();
            configure?.Invoke(jasperRegistry);

            return new JasperStorytellerHost<JasperHttpRegistry>(jasperRegistry);
        }
    }

    public class JasperStorytellerHost<T> : ISystem where T : JasperRegistry
    {
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

            Registry.Services.AddSingleton<MessageTrackingLogger>();


            registry.Services.AddSingleton<MessageHistory>();
            registry.Services.AddSingleton<IMessageLogger, StorytellerMessageLogger>();

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
                _messageLogger.ServiceName = _runtime.ServiceName;

                _messageLogger = _runtime.Get<IMessageLogger>().As<StorytellerMessageLogger>();
                beforeAll();
            });

            return _warmup;
        }


        public class JasperContext : IExecutionContext
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
}
