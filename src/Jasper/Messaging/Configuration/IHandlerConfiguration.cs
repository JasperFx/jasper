using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;

namespace Jasper.Messaging.Configuration
{
    public interface IHandlerConfiguration : IHasErrorHandlers
    {
        /// <summary>
        /// Configure how Jasper discovers message handler classes to override
        /// the built in conventions
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        IHandlerConfiguration Discovery(Action<HandlerSource> configure);


        /// <summary>
        /// Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void GlobalPolicy<T>() where T : IHandlerPolicy, new();

        /// <summary>
        /// Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        void GlobalPolicy(IHandlerPolicy policy);

        /// <summary>
        /// The default number of attempts to try to process a received message
        /// if there is no explicit configuration at the chain level. The default is 1
        /// </summary>
        int DefaultMaximumAttempts { get; set; }
    }


    public class HandlerConfiguration : IHandlerConfiguration
    {
        internal readonly HandlerSource Source = new HandlerSource();

        public IHandlerConfiguration Discovery(Action<HandlerSource> configure)
        {
            configure(Source);
            return this;
        }

        private readonly IList<IHandlerPolicy> _globals = new List<IHandlerPolicy>();

        // TODO -- have a Local option later
        /// <summary>
        /// Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void GlobalPolicy<T>() where T : IHandlerPolicy, new()
        {
            GlobalPolicy(new T());
        }

        /// <summary>
        /// Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        public void GlobalPolicy(IHandlerPolicy policy)
        {
            _globals.Add(policy);
        }


        internal void ApplyPolicies(HandlerGraph graph)
        {
            foreach (var policy in _globals)
            {
                policy.Apply(graph);
            }

            graph.ErrorHandlers.AddRange(this.As<IHasErrorHandlers>().ErrorHandlers);

            foreach (var chain in graph.Chains)
            {
                chain.MaximumAttempts = DefaultMaximumAttempts;
            }
        }

        /// <summary>
        /// The default number of attempts to try to process a received message
        /// if there is no explicit configuration at the chain level. The default is 1
        /// </summary>
        public int DefaultMaximumAttempts { get; set; } = 1;


        public IList<IErrorHandler> ErrorHandlers { get; } = new List<IErrorHandler>();
    }

}
