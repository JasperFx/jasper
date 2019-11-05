using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper
{
    /// <summary>
    /// Configuration for a single message listener within a Jasper application
    /// </summary>
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
        public string Scheme => Uri.Scheme;

        public int Port => Uri.Port;

        protected bool Equals(ListenerSettings other)
        {
            return Equals(Uri, other.Uri);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ListenerSettings) obj);
        }

        public override int GetHashCode()
        {
            return (Uri != null ? Uri.GetHashCode() : 0);
        }

        IListenerSettings IListenerSettings.MaximumParallelization(int maximumParallelHandlers)
        {
            ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this;
        }

        IListenerSettings IListenerSettings.Sequential()
        {
            ExecutionOptions.MaxDegreeOfParallelism = 1;
            return this;
        }

        IListenerSettings IListenerSettings.IsDurable()
        {
            IsDurable = true;
            return this;
        }

        IListenerSettings IListenerSettings.IsNotDurable()
        {
            IsDurable = false;
            return this;
        }
    }
}
