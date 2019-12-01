using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper.Configuration
{
    /// <summary>
    /// Configuration for a single message listener within a Jasper application
    /// </summary>
    public abstract class Endpoint
    {
        protected Endpoint()
        {
        }

        protected Endpoint(Uri uri)
        {
            Parse(uri);
            Uri = uri;
        }

        /// <summary>
        /// Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name { get; set; }


        public abstract void Parse(Uri uri);

        /// <summary>
        /// The actual address of the listener, including the transport scheme
        /// </summary>
        public Uri Uri { get; protected set; }

        /// <summary>
        /// Mark whether or not the receiver for this listener should use
        /// message persistence for durability
        /// </summary>
        public bool IsDurable { get; set; }

        public ExecutionDataflowBlockOptions ExecutionOptions { get; set; } = new ExecutionDataflowBlockOptions();

        public bool IsListener { get; set; }
    }
}
