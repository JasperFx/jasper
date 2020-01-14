using System;
using System.Threading.Tasks.Dataflow;
using LamarCodeGeneration.Util;

namespace Jasper.Configuration
{
    internal class ListenerConfiguration : ListenerConfiguration<IListenerConfiguration, Endpoint>, IListenerConfiguration
    {
        public ListenerConfiguration(Endpoint endpoint) : base(endpoint)
        {
        }
    }

    public class ListenerConfiguration<TSelf, TEndpoint> : IListenerConfiguration<TSelf> where TSelf : IListenerConfiguration<TSelf> where TEndpoint : Endpoint
    {
        public ListenerConfiguration(TEndpoint endpoint)
        {
            this.endpoint = endpoint;
            endpoint.IsListener = true;


        }

        protected TEndpoint endpoint { get; }

        public TSelf MaximumThreads(int maximumParallelHandlers)
        {
            endpoint.ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this.As<TSelf>();
        }

        public TSelf Sequential()
        {
            endpoint.ExecutionOptions.MaxDegreeOfParallelism = 1;
            endpoint.ExecutionOptions.EnsureOrdered = true;
            return this.As<TSelf>();
        }

        public TSelf Durable()
        {
            endpoint.IsDurable = true;
            return this.As<TSelf>();
        }

        public TSelf NotDurable()
        {
            endpoint.IsDurable = false;
            return this.As<TSelf>();
        }

        public TSelf ConfigureExecution(Action<ExecutionDataflowBlockOptions> configure)
        {
            configure(endpoint.ExecutionOptions);
            return this.As<TSelf>();
        }

        public TSelf UseForReplies()
        {
            endpoint.IsUsedForReplies = true;
            return this.As<TSelf>();
        }
    }
}
