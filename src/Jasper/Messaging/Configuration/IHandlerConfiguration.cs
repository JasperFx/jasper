using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using LamarCodeGeneration;

namespace Jasper.Messaging.Configuration
{
    public interface IHandlerConfiguration : IHasRetryPolicies
    {
        /// <summary>
        ///     Configure how Jasper discovers message handler classes to override
        ///     the built in conventions
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        IHandlerConfiguration Discovery(Action<HandlerSource> configure);


        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void GlobalPolicy<T>() where T : IHandlerPolicy, new();

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        void GlobalPolicy(IHandlerPolicy policy);

    }


    public class HandlerConfiguration : IHandlerConfiguration
    {
        private readonly IList<IHandlerPolicy> _globals = new List<IHandlerPolicy>();
        private readonly HandlerGraph _graph;
        internal readonly HandlerSource Source = new HandlerSource();

        public HandlerConfiguration(HandlerGraph graph)
        {
            _graph = graph;
        }

        public IHandlerConfiguration Discovery(Action<HandlerSource> configure)
        {
            configure(Source);
            return this;
        }

        // TODO -- have a Local option later
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

        /// <summary>
        /// Polly policies for how Jasper should deal with message failures
        /// </summary>
        public RetryPolicyCollection Retries
        {
            get => _graph.Retries;
            set => _graph.Retries = value;
        }


        internal void ApplyPolicies(GenerationRules rules, IContainer container)
        {
            foreach (var policy in _globals) policy.Apply(_graph, rules, container);
        }
    }
}
