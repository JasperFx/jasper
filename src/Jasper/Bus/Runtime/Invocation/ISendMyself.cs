namespace Jasper.Bus.Runtime.Invocation
{
    public interface ISendMyself
    {
        Envelope CreateEnvelope(Envelope original);
    }
}