namespace Jasper.Remotes.Messaging
{
    public interface IListener
    {
    }

    public interface IListener<T> : IListener
    {
        void Receive(T message);
    }
}
