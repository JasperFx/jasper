using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime.Routing;
using Jasper.Util;
using Lamar;

namespace Jasper.Runtime;

public class MessagePublisher : CommandBus, IMessagePublisher
{
    [DefaultConstructor]
    public MessagePublisher(IJasperRuntime runtime) : base(runtime)
    {
    }

    public MessagePublisher(IJasperRuntime runtime, string? correlationId) : base(runtime, correlationId)
    {
    }

    public Task SendAsync<T>(T message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var outgoing = Runtime.Router.RouteOutgoingByMessage(message);
        trackEnvelopeCorrelation(outgoing);

        if (!outgoing.Any())
        {
            throw new NoRoutesException(typeof(T));
        }

        return persistOrSendAsync(outgoing);
    }

    public Task PublishEnvelopeAsync(Envelope envelope)
    {
        if (envelope.Message == null && envelope.Data == null)
        {
            throw new ArgumentNullException(nameof(envelope.Message));
        }

        var outgoing = Runtime.Router.RouteOutgoingByEnvelope(envelope);
        trackEnvelopeCorrelation(outgoing);

        if (outgoing.Any())
        {
            return persistOrSendAsync(outgoing);
        }

        Runtime.MessageLogger.NoRoutesFor(envelope);
        return Task.CompletedTask;
    }

    public Task PublishAsync<T>(T message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var envelope = new Envelope(message);

        return PublishEnvelopeAsync(envelope);
    }

    public async Task<Guid> SendEnvelopeAsync(Envelope envelope)
    {
        if (envelope.Message == null && envelope.Data == null)
        {
            throw new ArgumentNullException(nameof(envelope.Message));
        }

        var outgoing = Runtime.Router.RouteOutgoingByEnvelope(envelope);

        trackEnvelopeCorrelation(outgoing);

        if (!outgoing.Any())
        {
            Runtime.MessageLogger.NoRoutesFor(envelope);

            throw new NoRoutesException(envelope);
        }

        await persistOrSendAsync(outgoing);

        return envelope.Id;
    }

    public Task SendAndExpectResponseForAsync<TResponse>(object message, Action<Envelope>? customization = null)
    {
        var envelope = EnvelopeForRequestResponse<TResponse>(message);

        customization?.Invoke(envelope);

        return SendEnvelopeAsync(envelope);
    }

    public Task SendToTopicAsync(object message, string topicName)
    {
        var envelope = new Envelope(message)
        {
            TopicName = topicName
        };

        var outgoing = Runtime.Router.RouteToTopic(topicName, envelope);
        return persistOrSendAsync(outgoing);
    }

    /// <summary>
    ///     Send to a specific destination rather than running the routing rules
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="destination">The destination to send to</param>
    /// <param name="message"></param>
    public Task SendToDestinationAsync<T>(Uri destination, T message)
    {
        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var envelope = new Envelope { Message = message, Destination = destination };
        Runtime.Router.RouteToDestination(destination, envelope);

        trackEnvelopeCorrelation(envelope);

        return persistOrSendAsync(envelope);
    }

    /// <summary>
    ///     Send a message that should be executed at the given time
    /// </summary>
    /// <param name="message"></param>
    /// <param name="time"></param>
    /// <typeparam name="T"></typeparam>
    public Task ScheduleSendAsync<T>(T message, DateTimeOffset time)
    {
        return SendEnvelopeAsync(new Envelope
        {
            Message = message,
            ScheduledTime = time.ToUniversalTime(),
            Status = EnvelopeStatus.Scheduled
        });
    }

    /// <summary>
    ///     Send a message that should be executed after the given delay
    /// </summary>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    /// <typeparam name="T"></typeparam>
    public Task ScheduleSendAsync<T>(T message, TimeSpan delay)
    {
        return ScheduleSendAsync(message, DateTime.UtcNow.Add(delay));
    }

    public Envelope EnvelopeForRequestResponse<TResponse>(object request)
    {
        return new Envelope
        {
            Message = request,
            ReplyRequested = typeof(TResponse).ToMessageTypeName(), // memoize this maybe?
            AcceptedContentTypes =
                new[] { EnvelopeConstants.JsonContentType } // TODO -- might want a default serializer option for here
        };
    }

    private void trackEnvelopeCorrelation(Envelope[] outgoing)
    {
        foreach (var outbound in outgoing) trackEnvelopeCorrelation(outbound);
    }

    protected virtual void trackEnvelopeCorrelation(Envelope outbound)
    {
        outbound.Source = Runtime.Advanced.ServiceName;
        outbound.CorrelationId = CorrelationId;
    }

    private Task persistOrSendAsync(Envelope envelope)
    {
        if (envelope.Sender == null)
        {
            throw new InvalidOperationException("This envelope has not been routed (Sender is null)");
        }

        if (Transaction != null)
        {
            _outstanding.Fill(envelope);
            return envelope.Sender.IsDurable ? Transaction.PersistAsync(envelope) : Task.CompletedTask;
        }

        return envelope.StoreAndForwardAsync();
    }

    private async Task persistOrSendAsync(params Envelope[] outgoing)
    {
        if (Transaction != null)
        {
            await Transaction.PersistAsync(outgoing.Where(isDurable).ToArray());

            _outstanding.Fill(outgoing);
        }
        else
        {
            foreach (var outgoingEnvelope in outgoing) await outgoingEnvelope.StoreAndForwardAsync();
        }
    }

    private bool isDurable(Envelope envelope)
    {
        // TODO -- should this be memoized? The test on envelope Destination anyway
        return envelope.Sender?.IsDurable ?? Runtime.Endpoints.GetOrBuildSendingAgent(envelope.Destination!).IsDurable;
    }
}
