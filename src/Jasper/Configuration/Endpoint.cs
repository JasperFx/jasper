using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper.Configuration
{
    /// <summary>
    /// Configuration for a single message listener within a Jasper application
    /// </summary>
    public class Endpoint
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


    }
}
