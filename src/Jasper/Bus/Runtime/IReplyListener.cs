namespace Jasper.Bus.Runtime
{
    public interface IReplyListener
    {
        void Handle(Envelope envelope);
    }
}