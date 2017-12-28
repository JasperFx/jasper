using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using BlueMilk.Codegen;
using Jasper.Bus.ErrorHandling;
using StructureMap;

namespace Jasper.Bus.Model
{
    public class HandlerGraph : HandlerSet<HandlerChain, MessageHandler>, IHasErrorHandlers
    {
        public static readonly string Context = "context";

        private bool _hasGrouped = false;
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();
        private readonly Dictionary<Type, HandlerChain> _chains = new Dictionary<Type, HandlerChain>();
        private readonly Dictionary<Type, MessageHandler> _handlers = new Dictionary<Type, MessageHandler>();

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

        public MessageHandler HandlerFor(Type messageType)
        {
            return _handlers.ContainsKey(messageType) ? _handlers[messageType] : null;
        }

        public MessageHandler HandlerFor<T>()
        {
            return HandlerFor(typeof(T));
        }

        public HandlerChain ChainFor(Type messageType)
        {
            return _chains.ContainsKey(messageType) ? _chains[messageType] : null;
        }

        public HandlerChain ChainFor<T>()
        {
            return ChainFor(typeof(T));
        }

        protected override HandlerChain[] chains => _chains.Values.ToArray();
        public HandlerChain[] Chains => _chains.Values.ToArray();

        internal void Compile(GenerationRules generation, JasperRuntime runtime)
        {
            if (!_hasGrouped)
            {
                Group();
            }

            var handlers = CompileAndBuildAll(generation, runtime.Get);
            foreach (var handler in handlers)
            {
                _handlers.Add(handler.Chain.MessageType, handler);
            }
        }

        protected override void beforeGeneratingCode()
        {
            if (!_hasGrouped)
            {
                Group();
            }
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
