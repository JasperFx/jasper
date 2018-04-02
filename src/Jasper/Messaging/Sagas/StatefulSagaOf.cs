namespace Jasper.Messaging.Sagas
{
    /// <summary>
    /// Base class that just
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public abstract class StatefulSagaOf<TState>
    {
        public bool IsCompleted {get; protected set;}
        public void MarkCompleted() => IsCompleted = true;
    }
}
