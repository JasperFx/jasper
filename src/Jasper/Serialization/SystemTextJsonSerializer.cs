using System;
using System.Text.Json;

namespace Jasper.Serialization;

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

    public byte[] Write(object message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, _options);
    }

    public object ReadFromData(Type messageType, byte[] data)
    {
        return JsonSerializer.Deserialize(data, messageType)!;
    }

    public object ReadFromData(byte[]? data)
    {
        throw new NotSupportedException("System.Text.Json requires a known message type");
    }
}
