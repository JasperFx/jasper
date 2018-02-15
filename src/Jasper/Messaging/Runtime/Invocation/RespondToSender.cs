namespace Jasper.Messaging.Runtime.Invocation
{
    public class RespondToSender : ISendMyself
    {
        public RespondToSender(object outgoing)
        {
            Outgoing = outgoing;
        }

        public Envelope CreateEnvelope(Envelope original)
        {
            var response = original.ForResponse(Outgoing);
            response.Destination = original.ReplyUri;

            return response;
        }

        public object Outgoing { get; }
    }
}