using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime.WorkerQueues;

public class InlineReceiver : IReceiver
{
    private readonly ILogger _logger;
    private readonly IHandlerPipeline _pipeline;
    private readonly AdvancedSettings _settings;

    public InlineReceiver(IJasperRuntime runtime, IHandlerPipeline pipeline)
    {
        _pipeline = pipeline;
        _logger = runtime.Logger;
        _settings = runtime.Advanced;
    }

    public void Dispose()
    {
        // Nothing
    }

    public async ValueTask ReceivedAsync(IListener listener, Envelope[] messages)
    {
        foreach (var envelope in messages) await ReceivedAsync(listener, envelope);
    }

    public async ValueTask ReceivedAsync(IListener listener, Envelope envelope)
    {
        using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryReceiveSpanName!, envelope,
            ActivityKind.Consumer);

        try
        {
            envelope.MarkReceived(listener, DateTimeOffset.UtcNow, _settings);
            await _pipeline.InvokeAsync(envelope, listener, activity!);
            _logger.IncomingReceived(envelope, listener.Address);

            // TODO -- mark success on the activity?
        }
        catch (Exception? e)
        {
            // TODO -- Mark failures onto the activity?
            _logger.LogError(e, "Failure to receive an incoming message for envelope {EnvelopeId}", envelope.Id);

            try
            {
                await listener.DeferAsync(envelope);
            }
            catch (Exception? ex)
            {
                _logger.LogError(ex,
                    "Error when trying to Nack a Rabbit MQ message that failed in the HandlerPipeline ({CausationId})",
                    envelope.CorrelationId);
            }
        }
    }
}
