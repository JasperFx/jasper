using System;
using Baseline;
using Jasper.Serialization;
using Newtonsoft.Json;
using TypeExtensions = LamarCodeGeneration.Util.TypeExtensions;

namespace Jasper.Configuration;

public class SubscriberConfiguration<T, TEndpoint> : ISubscriberConfiguration<T>
    where TEndpoint : Endpoint where T : ISubscriberConfiguration<T>
{
    // ReSharper disable once InconsistentNaming
    protected readonly TEndpoint _endpoint;

    public SubscriberConfiguration(TEndpoint endpoint)
    {
        _endpoint = endpoint;
    }

    protected TEndpoint Endpoint => _endpoint;

    public T Durably()
    {
        _endpoint.Mode = EndpointMode.Durable;
        return TypeExtensions.As<T>(this);
    }


    public T BufferedInMemory()
    {
        _endpoint.Mode = EndpointMode.BufferedInMemory;
        return this.As<T>();
    }

    public T SendInline()
    {
        _endpoint.Mode = EndpointMode.Inline;
        return this.As<T>();
    }

    public T Named(string name)
    {
        _endpoint.Name = name;
        return this.As<T>();
    }

    public ISubscriberConfiguration<T> CustomNewtonsoftJsonSerialization(JsonSerializerSettings customSettings)
    {
        var serializer = new NewtonsoftSerializer(customSettings);
        _endpoint.RegisterSerializer(serializer);
        return this;
    }

    public ISubscriberConfiguration<T> DefaultSerializer(IMessageSerializer serializer)
    {
        _endpoint.RegisterSerializer(serializer);
        _endpoint.DefaultSerializer = serializer;
        return this.As<T>();
    }

    public T CustomizeOutgoing(Action<Envelope> customize)
    {
        _endpoint.Customizations.Add(customize);
        return TypeExtensions.As<T>(this);
    }

    public T CustomizeOutgoingMessagesOfType<TMessage>(Action<Envelope> customize)
    {
        return CustomizeOutgoingMessagesOfType<TMessage>((env, _) => customize(env));
    }

    public T CustomizeOutgoingMessagesOfType<TMessage>(Action<Envelope, TMessage> customize)
    {
        return CustomizeOutgoing(env =>
        {
            if (env.Message is TMessage message)
            {
                customize(env, message);
            }
        });
    }


    /// <summary>
    ///     Fine-tune the circuit breaker parameters for this outgoing subscriber endpoint
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    public T CircuitBreaking(Action<ICircuitParameters> configure)
    {
        configure(_endpoint);
        return this.As<T>();
    }
}

internal class SubscriberConfiguration : SubscriberConfiguration<ISubscriberConfiguration, Endpoint>,
    ISubscriberConfiguration
{
    public SubscriberConfiguration(Endpoint endpoint) : base(endpoint)
    {
    }
}
