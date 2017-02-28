using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Codegen;
using JasperBus.Runtime.Invocation;
using StructureMap;

namespace JasperBus.Model
{
    public class MessageHandlerGraph : HandlerSet<HandlerChain, IInvocationContext, MessageHandler>
    {
        public static readonly string Context = "context";

        private bool _hasGrouped = false;
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();
        private readonly Dictionary<Type, HandlerChain> _chains = new Dictionary<Type, HandlerChain>();
        private readonly Dictionary<Type, MessageHandler> _handlers = new Dictionary<Type, MessageHandler>();

        public MessageHandlerGraph(GenerationConfig generation) : base(generation)
        {
            
        }

        private void assertNotGrouped()
        {
            if (_hasGrouped) throw new InvalidOperationException("This HandlerGraph has already been grouped");
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

        protected override HandlerChain[] chains => _chains.Values.ToArray();

        public void Compile(IContainer container)
        {
            if (!_hasGrouped)
            {
                Group();
            }

            var handlers = CompileAndBuildAll(container);
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
    }
}