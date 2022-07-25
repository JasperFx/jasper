using System;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Transports.Local;

public class LocalQueueSettings : Endpoint
{
    public LocalQueueSettings(string name)
    {
        Name = name.ToLowerInvariant();
    }

    public LocalQueueSettings(Uri uri) : base(uri)
    {
    }

    public override Uri Uri => $"local://{Name}".ToUri();

    public override void Parse(Uri uri)
    {
        Name = LocalTransport.QueueName(uri);
        Mode = uri.IsDurable() ? EndpointMode.Durable : EndpointMode.BufferedInMemory;
    }

    public override Uri CorrectedUriForReplies()
    {
        return Mode == EndpointMode.Durable ? $"local://durable/{Name}".ToUri() : $"local://{Name}".ToUri();
    }

    public override IListener BuildListener(IJasperRuntime runtime, IReceiver receiver)
    {
        throw new NotSupportedException();
    }

    protected override ISender CreateSender(IJasperRuntime runtime)
    {
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        return $"Local Queue '{Name}'";
    }
}
