using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using BaselineTypeDiscovery;
using Jasper.Conneg;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCompiler;

namespace Jasper.Messaging.Model
{
    public class HandlerGraph : IHasRetryPolicies, IGeneratesCode
    {
        public static readonly string Context = "context";
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();

        private readonly object _groupingLock = new object();

        private ImHashMap<Type, HandlerChain> _chains = ImHashMap<Type, HandlerChain>.Empty;


        private GenerationRules _generation;
        private ImHashMap<Type, MessageHandler> _handlers = ImHashMap<Type, MessageHandler>.Empty;

        private bool _hasGrouped;

        public HandlerGraph()
        {
            // All of this is to seed the handler and its associated retry policies
            // for scheduling outgoing messages
            _handlers = _handlers.AddOrUpdate(typeof(Envelope), new ScheduledSendEnvelopeHandler());
            Configuration = new HandlerConfiguration(this);
        }

        internal HandlerConfiguration Configuration { get; }

        internal IContainer Container { get; set; }

        /// <summary>
        ///     Policies and routing for local message handling
        /// </summary>
        public WorkersGraph Workers { get; } = new WorkersGraph();

        public HandlerChain[] Chains => _chains.Enumerate().Select(x => x.Value).ToArray();

        public RetryPolicyCollection Retries { get; set; } = new RetryPolicyCollection();

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


        public MessageHandler HandlerFor(Type messageType)
        {
            if (_handlers.TryFind(messageType, out var handler)) return handler;


            if (_chains.TryFind(messageType, out var chain))
            {
                if (chain.Handler != null)
                    handler = chain.Handler;
                else
                    lock (chain)
                    {
                        if (chain.Handler == null)
                        {
                            var generatedAssembly = new GeneratedAssembly(_generation);

                            chain.AssembleType(_generation, generatedAssembly, Container);

                            new AssemblyGenerator().Compile(generatedAssembly, Container.CreateServiceVariableSource());

                            handler = chain.CreateHandler(Container);
                        }
                        else
                        {
                            handler = chain.Handler;
                        }
                    }

                _handlers = _handlers.AddOrUpdate(messageType, handler);

                return handler;
            }

            // memoize the "miss"
            _handlers = _handlers.AddOrUpdate(messageType, null);
            return null;
        }



        internal void Compile(GenerationRules generation, IContainer container)
        {
            _generation = generation;
            Container = container;

            var forwarders = container.GetInstance<Forwarders>();
            AddForwarders(forwarders);
        }

        public void Group()
        {
            lock (_groupingLock)
            {
                if (_hasGrouped) return;

                _calls.Where(x => x.MessageType.IsConcrete())
                    .GroupBy(x => x.MessageType)
                    .Select(group => new HandlerChain(group))
                    .Each(chain => { _chains = _chains.AddOrUpdate(chain.MessageType, chain); });

                _calls.Where(x => !x.MessageType.IsConcrete())
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
                        typeof(ForwardingHandler<,>).CloseAndBuildAs<MessageHandler>(this, source, destination);

                    _chains = _chains.AddOrUpdate(source, handler.Chain);
                    _handlers = _handlers.AddOrUpdate(source, handler);
                }
            }
        }

        public bool CanHandle(Type messageType)
        {
            return _chains.TryFind(messageType, out var chain);
        }

        public string[] ValidMessageTypeNames()
        {
            return Chains.Select(x => x.MessageType.ToMessageTypeName()).ToArray();
        }

        IServiceVariableSource IGeneratesCode.AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly)
        {
            foreach (var chain in Chains)
            {
                chain.AssembleType(rules, assembly, Container);
            }

            return Container.CreateServiceVariableSource();
        }

        async Task IGeneratesCode.AttachPreBuiltTypes(GenerationRules rules, Assembly assembly, IServiceProvider services)
        {
            var typeSet = await TypeRepository.ForAssembly(assembly);
            var handlerTypes = typeSet.ClosedTypes.Concretes.Where(x => x.CanBeCastTo<MessageHandler>()).ToArray();

            var container = (IContainer)services;

            foreach (var chain in Chains)
            {
                var handler = chain.AttachPreBuiltHandler(rules, container, handlerTypes);
                if (handler != null) _handlers = _handlers.Update(chain.MessageType, handler);
            }
        }

        Task IGeneratesCode.AttachGeneratedTypes(GenerationRules rules, IServiceProvider services)
        {
            foreach (var chain in Chains)
            {
                var handler = chain.CreateHandler((IContainer) services);
                _handlers = _handlers.Update(chain.MessageType, handler);
            }

            return Task.CompletedTask;
        }

        string IGeneratesCode.CodeType => "Handlers";
    }
}
