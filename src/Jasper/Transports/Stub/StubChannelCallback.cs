﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jasper.Transports.Stub;

public class StubChannelCallback : IChannelCallback
{
    private readonly StubEndpoint _endpoint;
    private readonly Envelope _envelope;
    public readonly IList<Envelope> Sent = new List<Envelope>();

    public StubChannelCallback(StubEndpoint endpoint, Envelope envelope)
    {
        _endpoint = endpoint;
        _envelope = envelope;
    }


    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool Completed { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool Deferred { get; set; }

    public ValueTask CompleteAsync(Envelope envelope)
    {
        Completed = true;
        return ValueTask.CompletedTask;
    }

    public async ValueTask DeferAsync(Envelope envelope)
    {
        Deferred = true;
        await _endpoint.EnqueueOutgoingAsync(_envelope);
    }
}
