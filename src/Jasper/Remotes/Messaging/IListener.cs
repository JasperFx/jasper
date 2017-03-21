namespace Jasper.Remotes.Messaging
{
    public interface IListener
    {
        void Receive<T>(T message);
    }

    public interface IListener<T>
    {
        void Receive(T message);
    }
}
