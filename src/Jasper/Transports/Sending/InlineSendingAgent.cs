using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;

namespace Jasper.Transports.Sending;

public class InlineSendingAgent : ISendingAgent
{
    private readonly IMessageLogger _logger;
    private readonly ISender _sender;
    private readonly AdvancedSettings _settings;

    public InlineSendingAgent(ISender sender, Endpoint endpoint, IMessageLogger logger, AdvancedSettings settings)
    {
        _sender = sender;
        _logger = logger;
        _settings = settings;
        Endpoint = endpoint;
    }

    public void Dispose()
    {
        // nothing
    }

    public Uri Destination => _sender.Destination;
    public Uri? ReplyUri { get; set; }
    public bool Latched { get; } = false;
    public bool IsDurable { get; } = false;
    public bool SupportsNativeScheduledSend => _sender.SupportsNativeScheduledSend;

    public async Task EnqueueOutgoingAsync(Envelope envelope)
    {
        setDefaults(envelope);
        await _sender.SendAsync(envelope);
        _logger.Sent(envelope);
    }

    public Task StoreAndForwardAsync(Envelope envelope)
    {
        return EnqueueOutgoingAsync(envelope);
    }

    public Endpoint Endpoint { get; }

    private void setDefaults(Envelope envelope)
    {
        envelope.Status = EnvelopeStatus.Outgoing;
        envelope.OwnerId = _settings.UniqueNodeId;
        envelope.ReplyUri ??= ReplyUri;
    }
}
