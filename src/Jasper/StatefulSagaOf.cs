namespace Jasper
{
    /// <summary>
    ///     Base class that just
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public abstract class StatefulSagaOf<TState>
    {
        public bool IsCompleted { get; protected set; }

        public void MarkCompleted()
        {
            IsCompleted = true;
        }

        /// <summary>
        /// The current state of the saga
        /// </summary>
        public TState State { get; set; }
    }
}
