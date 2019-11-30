using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper
{
    /// <summary>
    /// Configuration for a single message listener within a Jasper application
    /// </summary>
    [Obsolete("Going to fold into Endpoint")]
    public class ListenerSettings : IListenerSettings
    {
        /// <summary>
        /// Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The actual address of the listener, including the transport scheme
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Mark whether or not the receiver for this listener should use
        /// message persistence for durability
        /// </summary>
        public bool IsDurable { get; set; }

        public ExecutionDataflowBlockOptions ExecutionOptions { get; set; } = new ExecutionDataflowBlockOptions();


        IListenerSettings IListenerSettings.MaximumThreads(int maximumParallelHandlers)
        {
            ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this;
        }

        IListenerSettings IListenerSettings.Sequential()
        {
            ExecutionOptions.MaxDegreeOfParallelism = 1;
            return this;
        }

        IListenerSettings IListenerSettings.Durably()
        {
            IsDurable = true;
            return this;
        }

        IListenerSettings IListenerSettings.Lightweight()
        {
            IsDurable = false;
            return this;
        }
    }
}
