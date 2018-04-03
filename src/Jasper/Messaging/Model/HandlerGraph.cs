using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Sagas;
using Jasper.Util;
using Lamar;
using Lamar.Codegen;
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


        private GenerationRules _generation;
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
                var generatedAssembly = new GeneratedAssembly(_generation);
                chain.AssembleType(generatedAssembly);
                _container.CompileWithInlineServices(generatedAssembly);

                handler = chain.CreateHandler(_container);

                _handlers = _handlers.AddOrUpdate(messageType, handler);

                return handler;
            }

            // memoize the "miss"
            _handlers = _handlers.AddOrUpdate(messageType, null);
            return null;

        }

        /// <summary>
        /// The currently known strategy for persisting saga state
        /// </summary>
        public ISagaPersistence SagaPersistence { get; set; }

        internal void Compile(GenerationRules generation, JasperRuntime runtime, PerfTimer timer)
        {
            if (!_hasGrouped)
            {
                Group();
            }

            _generation = generation;
            _container = runtime.Container;
        }

        public void Group()
        {
            assertNotGrouped();

            _calls.Where(x => x.MessageType.IsConcrete())
                .GroupBy(x => x.MessageType)
                .Select(group => new HandlerChain(@group))
                .Each(chain =>
                {
                    _chains = _chains.AddOrUpdate(chain.MessageType, chain);
                });

            _calls.Where(x => !x.MessageType.IsConcrete())
                .Each(call =>
                {
                    Chains
                        .Where(c => call.CouldHandleOtherMessageType(c.MessageType))
                        .Each(c => { c.AddAbstractedHandler(call); });
                });

            _hasGrouped = true;
        }

        public IList<IErrorHandler> ErrorHandlers { get; } = new List<IErrorHandler>();

        public bool CanHandle(Type messageType)
        {
            return _chains.TryFind(messageType, out var chain);
        }

    }
}
