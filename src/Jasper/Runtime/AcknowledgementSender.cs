using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime.Routing;
using Jasper.Serialization;

namespace Jasper.Runtime;

public class AcknowledgementSender : IAcknowledgementSender
{
    private readonly IEnvelopeRouter _router;
    private readonly IMessageSerializer _writer;

    public AcknowledgementSender(IEnvelopeRouter router, IJasperRuntime root)
    {
        _router = router;
        _writer = root.Options.FindSerializer(EnvelopeConstants.JsonContentType);
    }

    public Envelope BuildAcknowledgement(Envelope envelope)
    {
        var ack = new Envelope(new Acknowledgement { CorrelationId = envelope.Id }, _writer)
        {
            CausationId = envelope.Id.ToString(),
            Destination = envelope.ReplyUri,
            SagaId = envelope.SagaId
        };

        return ack;
    }

    /// <summary>
    ///     Sends an acknowledgement back to the original sender
    /// </summary>
    /// <returns></returns>
    public Task SendAcknowledgementAsync(Envelope original)
    {
        if (!original.AckRequested && !original.ReplyRequested.IsNotEmpty())
        {
            return Task.CompletedTask;
        }

        if (original.ReplyUri == null) return Task.CompletedTask;

        var ack = BuildAcknowledgement(original);

        var envelope = new Envelope
        {
            CausationId = original.Id.ToString(),
            Destination = original.ReplyUri,
            Message = ack
        };

        _router.RouteToDestination(original.ReplyUri, envelope);
        return envelope.StoreAndForwardAsync();
    }

    /// <summary>
    ///     Send a failure acknowledgement back to the original
    ///     sending service
    /// </summary>
    /// <param name="original"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task SendFailureAcknowledgementAsync(Envelope original, string message)
    {
        // Can't do anything here.
        if (original.ReplyUri == null) return Task.CompletedTask;

        if (original.AckRequested || original.ReplyRequested.IsNotEmpty())
        {
            var envelope = new Envelope
            {
                CausationId = original.Id.ToString(),
                Destination = original.ReplyUri,
                Message = new FailureAcknowledgement
                {
                    CorrelationId = original.Id,
                    Message = message
                }
            };

            _router.RouteToDestination(original.ReplyUri, envelope);
            return envelope.StoreAndForwardAsync();
        }

        return Task.CompletedTask;
    }
}
