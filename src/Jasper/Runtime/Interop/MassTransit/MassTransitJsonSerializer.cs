using System;
using Jasper.Runtime.Serialization;
using Newtonsoft.Json;

namespace Jasper.Runtime.Interop.MassTransit;

public class MassTransitJsonSerializer : IMessageSerializer
{
    // TODO -- copy settings from MT
    private readonly IMessageSerializer _inner = new NewtonsoftSerializer(new JsonSerializerSettings());

    public string ContentType { get; } = "application/vnd.masstransit+json";
    public byte[] Write(Envelope envelope)
    {
        throw new NotImplementedException();
        //var body = new MassTransitEnvelope{Body = }
    }

    public object ReadFromData(Type messageType, Envelope envelope)
    {
        var wrappedType = typeof(MassTransitEnvelope<>).MakeGenericType(messageType);

        var mtEnvelope = (MassTransitEnvelope)_inner.ReadFromData(wrappedType, envelope);
        mtEnvelope.TransferData(envelope);

        return mtEnvelope.Body!;
    }

    public object ReadFromData(byte[] data)
    {
        throw new NotImplementedException();
    }
}
