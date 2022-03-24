namespace Jasper
{
    #region sample_StatefulSagaOf
    /// <summary>
    /// Base class for implementing handlers for a stateful saga
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public abstract class StatefulSagaOf<TState>
    {
        /// <summary>
        /// Is the current stateful saga complete? If so,
        /// Jasper will delete the State document at the end
        /// of the current
        /// </summary>
        public bool IsCompleted { get; protected set; }

        /// <summary>
        /// Called to mark this saga as "complete"
        /// </summary>
        public void MarkCompleted()
        {
            IsCompleted = true;
        }

        /// <summary>
        /// The current state document of the saga.
        /// </summary>
        public TState State { get; set; }
    }
    #endregion
}
