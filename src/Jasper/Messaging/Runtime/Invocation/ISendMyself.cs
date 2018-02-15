namespace Jasper.Messaging.Runtime.Invocation
{
    public interface ISendMyself
    {
        Envelope CreateEnvelope(Envelope original);
    }
}
