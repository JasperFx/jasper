using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Serialization;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports.Local;

public class DurableLocalSendingAgent : DurableWorkerQueue, ISendingAgent
{
    private readonly IMessageLogger _messageLogger;
    private readonly IEnvelopePersistence _persistence;
    private readonly IMessageSerializer _serializer;
    private readonly AdvancedSettings _settings;

    public DurableLocalSendingAgent(Endpoint endpoint, IHandlerPipeline pipeline,
        AdvancedSettings settings, IEnvelopePersistence persistence, ILogger logger,
        IMessageLogger messageLogger) : base(endpoint, pipeline, settings, persistence, logger)
    {
        _settings = settings;
        _persistence = persistence;
        _messageLogger = messageLogger;
        _serializer = endpoint.DefaultSerializer ??
                      throw new ArgumentOutOfRangeException(nameof(endpoint),
                          "No default serializer for this Endpoint");
        Destination = endpoint.Uri;

        Endpoint = endpoint;
        ReplyUri = TransportConstants.RepliesUri;

        Address = Destination;
    }

    public Uri Destination { get; }

    public Endpoint Endpoint { get; }

    public Uri? ReplyUri { get; set; }

    public bool Latched => false;

    public bool IsDurable => true;

    public ValueTask EnqueueOutgoingAsync(Envelope envelope)
    {
        _messageLogger.Sent(envelope);

        Enqueue(envelope);

        return ValueTask.CompletedTask;
    }

    public async ValueTask StoreAndForwardAsync(Envelope envelope)
    {
        _messageLogger.Sent(envelope);
        writeMessageData(envelope);

        // TODO -- have to watch this one
        envelope.Status = envelope.IsScheduledForLater(DateTimeOffset.Now)
            ? EnvelopeStatus.Scheduled
            : EnvelopeStatus.Incoming;

        envelope.OwnerId = envelope.Status == EnvelopeStatus.Incoming
            ? _settings.UniqueNodeId
            : TransportConstants.AnyNode;

        await _persistence.StoreIncomingAsync(envelope);

        if (envelope.Status == EnvelopeStatus.Incoming)
        {
            Enqueue(envelope);
        }
    }

    public bool SupportsNativeScheduledSend { get; } = true;


    private void writeMessageData(Envelope envelope)
    {
        if (envelope.Message is null)
        {
            throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is null");
        }

        if (envelope.Data == null || envelope.Data.Length == 0)
        {
            envelope.Data = _serializer.Write(envelope.Message);
            envelope.ContentType = _serializer.ContentType;
        }
    }
}
