using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Messaging.ErrorHandling;
using Jasper.Util;
using TypeExtensions = Baseline.TypeExtensions;
using Baseline;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using Lamar.Compilation;
using Lamar.Util;

namespace Jasper.Messaging.Model
{
    public class HandlerGraph : IHasErrorHandlers
    {
        public static readonly string Context = "context";

        private bool _hasGrouped = false;
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();

        private ImHashMap<Type, HandlerChain> _chains = ImHashMap<Type, HandlerChain>.Empty;
        private ImHashMap<Type, MessageHandler> _handlers = ImHashMap<Type, MessageHandler>.Empty;


        private JasperGenerationRules _generation;
        private IContainer _container;

        private void assertNotGrouped()
        {
            if (_hasGrouped) throw new InvalidOperationException("This HandlerGraph has already been grouped/compiled");
        }

        public void Add(HandlerCall call)
        {
            assertNotGrouped();
            _calls.Add(call);
        }

        public void AddRange(IEnumerable<HandlerCall> calls)
        {
            assertNotGrouped();
            _calls.AddRange(calls);
        }

        /// <summary>
        /// Policies and routing for local message handling
        /// </summary>
        public WorkersGraph Workers { get; } = new WorkersGraph();



        public MessageHandler HandlerFor<T>()
        {
            return HandlerFor(typeof(T));
        }

        public HandlerChain ChainFor(Type messageType)
        {
            return HandlerFor(messageType)?.Chain;
        }

        public HandlerChain ChainFor<T>()
        {
            return ChainFor(typeof(T));
        }

        public HandlerChain[] Chains => _chains.Enumerate().Select(x => x.Value).ToArray();



        public MessageHandler HandlerFor(Type messageType)
        {
            if (_handlers.TryFind(messageType, out var handler)) return handler;


            if (_chains.TryFind(messageType, out var chain))
            {
                if (chain.Handler != null)
                {
                    handler = chain.Handler;
                }
                else
                {
                    lock (chain)
                    {
                        if (chain.Handler == null)
                        {
                            var generatedAssembly = new GeneratedAssembly(_generation);
                            chain.AssembleType(generatedAssembly, _generation);
                            _container.CompileWithInlineServices(generatedAssembly);

                            handler = chain.CreateHandler(_container);
                        }
                        else
                        {
                            handler = chain.Handler;
                        }
                    }
                }

                _handlers = _handlers.AddOrUpdate(messageType, handler);

                return handler;
            }

            // memoize the "miss"
            _handlers = _handlers.AddOrUpdate(messageType, null);
            return null;

        }


        internal void Compile(JasperGenerationRules generation, JasperRuntime runtime, PerfTimer timer)
        {
            _generation = generation;
            _container = runtime.Container;

            var forwarders = runtime.Get<Forwarders>();
            AddForwarders(forwarders);
        }

        private readonly object _groupingLock = new object();

        public void Group()
        {
            lock (_groupingLock)
            {
                if (_hasGrouped) return;

                _calls.Where(x => TypeExtensions.IsConcrete(x.MessageType))
                    .GroupBy(x => x.MessageType)
                    .Select(group => new HandlerChain(@group))
                    .Each(chain =>
                    {
                        _chains = _chains.AddOrUpdate(chain.MessageType, chain);
                    });

                _calls.Where(x => !TypeExtensions.IsConcrete(x.MessageType))
                    .Each(call =>
                    {
                        Chains
                            .Where(c => call.CouldHandleOtherMessageType(c.MessageType))
                            .Each(c => { c.AddAbstractedHandler(call); });
                    });

                _hasGrouped = true;
            }


        }

        public void AddForwarders(Forwarders forwarders)
        {
            foreach (var pair in forwarders.Relationships)
            {
                var source = pair.Key;
                var destination = pair.Value;

                if (_chains.TryFind(destination, out var inner))
                {
                    var handler =
                        TypeExtensions.CloseAndBuildAs<MessageHandler>(typeof(ForwardingHandler<,>), this, new Type[]{source, destination});

                    _chains = _chains.AddOrUpdate(source, handler.Chain);
                    _handlers = _handlers.AddOrUpdate(source, handler);
                }
            }
        }

        public IList<IErrorHandler> ErrorHandlers { get; } = new List<IErrorHandler>();

        public bool CanHandle(Type messageType)
        {
            return _chains.TryFind(messageType, out var chain);
        }

    }
}
