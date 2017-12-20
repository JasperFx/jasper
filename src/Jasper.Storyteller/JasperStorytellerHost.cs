using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Tracking;
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

        public static JasperStorytellerHost<JasperRegistry> Basic()
        {
            return new JasperStorytellerHost<JasperRegistry>(new JasperRegistry());
        }
    }

    public class JasperStorytellerHost<T> : ISystem where T : JasperRegistry
    {
        public readonly MessageHistory MessageHistory = new MessageHistory();

        private readonly StorytellerMessageLogger _messageLogger = new StorytellerMessageLogger();
        private readonly StorytellerTransportLogger _transportLogger = new StorytellerTransportLogger();

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
            Registry.Logging.LogMessageEventsWith<MessageTrackingLogger>();
            Registry.Services.Add(new ServiceDescriptor(typeof(IMessageLogger), _messageLogger));
            Registry.Services.Add(new ServiceDescriptor(typeof(ITransportLogger), _transportLogger));
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
                _parent._transportLogger.Start(context, _parent._messageLogger.Errors);
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
