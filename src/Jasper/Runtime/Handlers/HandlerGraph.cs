using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using BaselineTypeDiscovery;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Jasper.Persistence.Sagas;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Util;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCompiler;

namespace Jasper.Runtime.Handlers
{
    public class HandlerGraph : IGeneratesCode, IHandlerConfiguration
    {
        public static readonly string Context = "context";
        private readonly List<HandlerCall> _calls = new List<HandlerCall>();

        private readonly object _groupingLock = new object();

        private ImHashMap<Type, HandlerChain> _chains = ImHashMap<Type, HandlerChain>.Empty;

        internal readonly HandlerSource Source = new HandlerSource();
        private readonly IList<IHandlerPolicy> _globals = new List<IHandlerPolicy>();

        private GenerationRules _generation;
        private ImHashMap<Type, MessageHandler> _handlers = ImHashMap<Type, MessageHandler>.Empty;

        private bool _hasGrouped;

        public HandlerGraph()
        {
            // All of this is to seed the handler and its associated retry policies
            // for scheduling outgoing messages
            _handlers = _handlers.AddOrUpdate(typeof(Envelope), new ScheduledSendEnvelopeHandler());

            GlobalPolicy<SagaFramePolicy>();
        }

        internal void StartCompiling(JasperOptions options)
        {
            Compiling = Source.FindCalls(options).ContinueWith(t =>
            {
                var calls = t.Result;

                if (calls != null && calls.Any()) AddRange(calls);

                Group();
            });
        }

        internal Task Compiling { get; private set; }


        internal IContainer Container { get; set; }

        public HandlerChain[] Chains => _chains.Enumerate().Select(x => x.Value).ToArray();

        public RetryPolicyCollection Retries { get; set; } = new RetryPolicyCollection();

        private void assertNotGrouped()
        {
            if (_hasGrouped) throw new InvalidOperationException("This HandlerGraph has already been grouped/compiled");
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

                            chain.AssembleType(_generation, generatedAssembly, Container);

                            new AssemblyGenerator().Compile(generatedAssembly, Container.CreateServiceVariableSource());

                            handler = chain.CreateHandler(Container);
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



        internal void Compile(GenerationRules generation, IContainer container)
        {
            foreach (var policy in _globals)
            {
                policy.Apply(this, generation, container);
            }

            _generation = generation;
            Container = container;

            var forwarders = container.GetInstance<Forwarders>();
            AddForwarders(forwarders);

            foreach (var configuration in _configurations)
            {
                configuration();
            }
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

        internal void AddForwarders(Forwarders forwarders)
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


        public IHandlerConfiguration Discovery(Action<HandlerSource> configure)
        {
            configure(Source);
            return this;
        }

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void GlobalPolicy<T>() where T : IHandlerPolicy, new()
        {
            GlobalPolicy(new T());
        }

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        public void GlobalPolicy(IHandlerPolicy policy)
        {
            _globals.Add(policy);
        }

        private readonly IList<Action> _configurations = new List<Action>();

        public void ConfigureHandlerForMessage<T>(Action<HandlerChain> configure)
        {
            ConfigureHandlerForMessage(typeof(T), configure);
        }

        public void ConfigureHandlerForMessage(Type messageType, Action<HandlerChain> configure)
        {
            _configurations.Add(() =>
            {
                var chain = ChainFor(messageType);
                if (chain != null)
                {
                    configure(chain);
                }
            });

        }
    }
}
