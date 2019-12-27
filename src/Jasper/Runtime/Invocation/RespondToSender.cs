namespace Jasper.Runtime.Invocation
{
    public class RespondToSender : ISendMyself
    {
        public RespondToSender(object outgoing)
        {
            Outgoing = outgoing;
        }

        public object Outgoing { get; }

        public Envelope CreateEnvelope(Envelope original)
        {
            var response = original.CreateForResponse(Outgoing);
            response.Destination = original.ReplyUri;

            return response;
        }
    }
}
