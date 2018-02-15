namespace Jasper.Messaging.Runtime
{
    public interface IReplyListener
    {
        void Handle(Envelope envelope);
    }
}