using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Codegen;
using JasperBus.Runtime.Invocation;
using StructureMap;

namespace JasperBus.Model
{
    public class HandlerGraph : HandlerSet<HandlerChain, IInvocationContext, MessageHandler>
    {
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();
        private readonly Dictionary<Type, HandlerChain> _chains = new Dictionary<Type, HandlerChain>();
        private readonly Dictionary<Type, MessageHandler> _handlers = new Dictionary<Type, MessageHandler>();

        public HandlerGraph(GenerationConfig generation) : base(generation, "context")
        {
            
        }

        public void Add(HandlerCall call)
        {
            _calls.Add(call);
        }

        public void AddRange(IEnumerable<HandlerCall> calls)
        {
            _calls.AddRange(calls);
        }

        public MessageHandler HandlerFor(Type messageType)
        {
            return _handlers.ContainsKey(messageType) ? _handlers[messageType] : null;
        }

        protected override HandlerChain[] chains => _chains.Values.ToArray();

        public void Compile(IContainer container)
        {
            _calls.Where(x => x.MessageType.IsConcrete())
                .GroupBy(x => x.MessageType)
                .Select(group => new HandlerChain(group))
                .Each(
                    chain =>
                    {
                        _chains.Add(chain.MessageType, chain);
                    });

            _calls.Where(x => !x.MessageType.IsConcrete())
                .Each(call =>
                {
                    _chains.Where(pair => call.CouldHandleOtherMessageType(pair.Key))
                        .Each(chain =>
                        {
                            chain.Value.AddAbstractedHandler(call);
                        });
                });

            var handlers = CompileAndBuildAll(container);
            foreach (var handler in handlers)
            {
                _handlers.Add(handler.Chain.MessageType, handler);
            }
        }
    }
}