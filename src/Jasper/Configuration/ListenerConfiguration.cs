using System;
using System.Threading.Tasks.Dataflow;
using LamarCodeGeneration.Util;

namespace Jasper.Configuration
{
    public class ListenerConfiguration : ListenerConfiguration<IListenerConfiguration>, IListenerConfiguration
    {
        public ListenerConfiguration(Endpoint endpoint) : base(endpoint)
        {
        }
    }

    public class ListenerConfiguration<TSelf> : IListenerConfiguration<TSelf> where TSelf : IListenerConfiguration<TSelf>
    {
        private readonly Endpoint _endpoint;

        public ListenerConfiguration(Endpoint endpoint)
        {
            _endpoint = endpoint;
            endpoint.IsListener = true;
        }

        public TSelf MaximumThreads(int maximumParallelHandlers)
        {
            _endpoint.ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this.As<TSelf>();
        }

        public TSelf Sequential()
        {
            _endpoint.ExecutionOptions.MaxDegreeOfParallelism = 1;
            _endpoint.ExecutionOptions.EnsureOrdered = true;
            return this.As<TSelf>();
        }

        public TSelf Durably()
        {
            _endpoint.IsDurable = true;
            return this.As<TSelf>();
        }

        public TSelf Lightweight()
        {
            _endpoint.IsDurable = false;
            return this.As<TSelf>();
        }

        public TSelf ConfigureExecution(Action<ExecutionDataflowBlockOptions> configure)
        {
            configure(_endpoint.ExecutionOptions);
            return this.As<TSelf>();
        }

        public TSelf UseForReplies()
        {
            _endpoint.IsUsedForReplies = true;
            return this.As<TSelf>();
        }
    }
}
