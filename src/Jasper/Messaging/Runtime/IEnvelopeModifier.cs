namespace Jasper.Messaging.Runtime
{
    public interface IEnvelopeModifier
    {
        void Modify(Envelope envelope);
    }
}