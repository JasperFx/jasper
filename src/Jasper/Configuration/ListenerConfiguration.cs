using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper.Configuration
{
    public class ListenerConfiguration : IListenerConfiguration
    {
        private readonly Endpoint _endpoint;

        public ListenerConfiguration(Endpoint endpoint)
        {
            _endpoint = endpoint;
            endpoint.IsListener = true;
        }

        IListenerConfiguration IListenerConfiguration.MaximumThreads(int maximumParallelHandlers)
        {
            _endpoint.ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Sequential()
        {
            _endpoint.ExecutionOptions.MaxDegreeOfParallelism = 1;
            _endpoint.ExecutionOptions.EnsureOrdered = true;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Durably()
        {
            _endpoint.IsDurable = true;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Lightweight()
        {
            _endpoint.IsDurable = false;
            return this;
        }

        public IListenerConfiguration ConfigureExecution(Action<ExecutionDataflowBlockOptions> configure)
        {
            configure(_endpoint.ExecutionOptions);
            return this;
        }

        public IListenerConfiguration UseForReplies()
        {
            _endpoint.IsUsedForReplies = true;
            return this;
        }
    }
}
