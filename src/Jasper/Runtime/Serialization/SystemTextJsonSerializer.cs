using System;
using System.Text.Json;

namespace Jasper.Runtime.Serialization;

/// <summary>
///     Use System.Text.Json as the JSON serialization
/// </summary>
public class SystemTextJsonSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string ContentType { get; } = EnvelopeConstants.JsonContentType;

    public byte[] Write(Envelope envelope)
    {
        return JsonSerializer.SerializeToUtf8Bytes(envelope, _options);
    }

    public object ReadFromData(Type messageType, Envelope envelope)
    {
        return JsonSerializer.Deserialize(envelope.Data, messageType)!;
    }

    public object ReadFromData(byte[]? data)
    {
        throw new NotSupportedException("System.Text.Json requires a known message type");
    }
}
