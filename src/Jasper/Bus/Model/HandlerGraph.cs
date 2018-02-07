using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using BlueMilk;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using BlueMilk.Util;
using Jasper.Bus.ErrorHandling;
using Jasper.Util;

namespace Jasper.Bus.Model
{
    public class HandlerGraph : IHasErrorHandlers
    {
        public static readonly string Context = "context";

        private bool _hasGrouped = false;
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();
        private readonly Dictionary<Type, HandlerChain> _chains = new Dictionary<Type, HandlerChain>();
        private readonly Dictionary<Type, MessageHandler> _handlers = new Dictionary<Type, MessageHandler>();
        private GenerationRules _generation;
        private Container _container;

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

        public HandlerChain[] Chains => _chains.Values.ToArray();


        private readonly object _locker = new object();

        public MessageHandler HandlerFor(Type messageType)
        {
            if (!_handlers.ContainsKey(messageType))
            {
                lock (_locker)
                {
                    if (!_handlers.ContainsKey(messageType))
                    {
                        if (_chains.ContainsKey(messageType))
                        {
                            var chain = _chains[messageType];
                            var generatedAssembly = new GeneratedAssembly(_generation);
                            chain.AssembleType(generatedAssembly);
                            _container.CompileWithInlineServices(generatedAssembly);

                            var handler = chain.CreateHandler(_container);

                            _handlers.Add(messageType, handler);

                            return handler;
                        }
                        else
                        {
                            _handlers.Add(messageType, null); // memoize the "miss"

                            return null;
                        }
                    }
                }

                return _handlers[messageType];
            }

            return _handlers.ContainsKey(messageType) ? _handlers[messageType] : null;
        }

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
                .Each(
                    chain => { _chains.Add(chain.MessageType, chain); });

            _calls.Where(x => !x.MessageType.IsConcrete())
                .Each(call =>
                {
                    _chains.Where(pair => call.CouldHandleOtherMessageType(pair.Key))
                        .Each(chain => { chain.Value.AddAbstractedHandler(call); });
                });

            _hasGrouped = true;
        }

        public IList<IErrorHandler> ErrorHandlers { get; } = new List<IErrorHandler>();

        public bool CanHandle(Type messageType)
        {
            return _chains.ContainsKey(messageType);
        }

    }
}
