namespace JasperBus.Runtime
{
    public interface IReplyListener
    {
        void Handle(Envelope envelope);
    }
}