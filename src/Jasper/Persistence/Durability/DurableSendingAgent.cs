using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Jasper.Persistence.Durability;

public class DurableSendingAgent : SendingAgent
{
    private readonly ILogger _logger;
    private readonly IEnvelopePersistence _persistence;
    private readonly AsyncRetryPolicy _policy;

    private IList<Envelope> _queued = new List<Envelope>();

    public DurableSendingAgent(ISender sender, AdvancedSettings settings, ILogger logger,
        IMessageLogger messageLogger,
        IEnvelopePersistence persistence, Endpoint endpoint) : base(logger, messageLogger, sender, settings, endpoint)
    {
        _logger = logger;

        _persistence = persistence;

        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(i => (i * 100).Milliseconds()
                , (e, _) => { _logger.LogError(e, "Failed while trying to enqueue a message batch for retries"); });
    }

    public override bool IsDurable { get; } = true;

    public override Task EnqueueForRetryAsync(OutgoingMessageBatch batch)
    {
        return _policy.ExecuteAsync(_ => enqueueForRetryAsync(batch), _settings.Cancellation);
    }

    private async Task enqueueForRetryAsync(OutgoingMessageBatch batch)
    {
        if (_settings.Cancellation.IsCancellationRequested)
        {
            return;
        }

        var expiredInQueue = _queued.Where(x => x.IsExpired()).ToArray();
        var expiredInBatch = batch.Messages.Where(x => x.IsExpired()).ToArray();

        var expired = expiredInBatch.Concat(expiredInQueue).ToArray();
        var all = _queued.Where(x => !expiredInQueue.Contains(x))
            .Concat(batch.Messages.Where(x => !expiredInBatch.Contains(x)))
            .ToList();

        var reassigned = Array.Empty<Envelope>();
        if (all.Count > Endpoint.MaximumEnvelopeRetryStorage)
        {
            reassigned = all.Skip(Endpoint.MaximumEnvelopeRetryStorage).ToArray();
        }

        await _persistence.DiscardAndReassignOutgoingAsync(expired, reassigned, TransportConstants.AnyNode);
        _logger.DiscardedExpired(expired);

        _queued = all.Take(Endpoint.MaximumEnvelopeRetryStorage).ToList();
    }

    protected override async Task afterRestartingAsync(ISender sender)
    {
        var expired = _queued.Where(x => x.IsExpired()).ToArray();
        if (expired.Any())
        {
            await _persistence.DeleteIncomingEnvelopesAsync(expired);
        }

        var toRetry = _queued.Where(x => !x.IsExpired()).ToArray();
        _queued = new List<Envelope>();

        foreach (var envelope in toRetry) await _sender.SendAsync(envelope);
    }

    public override Task MarkSuccessfulAsync(OutgoingMessageBatch outgoing)
    {
        return _policy.ExecuteAsync(_ => _persistence.DeleteOutgoingAsync(outgoing.Messages.ToArray()),
            _settings.Cancellation);
    }

    public override Task MarkSuccessfulAsync(Envelope outgoing)
    {
        return _policy.ExecuteAsync(_ => _persistence.DeleteOutgoingAsync(outgoing), _settings.Cancellation);
    }

    protected override async Task storeAndForwardAsync(Envelope envelope)
    {
        await _persistence.StoreOutgoingAsync(envelope, _settings.UniqueNodeId);

        await _senderDelegate(envelope);
    }
}
