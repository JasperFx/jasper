using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime.WorkerQueues;

public class InlineWorkerQueue : IListeningWorkerQueue
{
    private readonly ILogger _logger;
    private readonly IHandlerPipeline _pipeline;
    private readonly AdvancedSettings _settings;

    public InlineWorkerQueue(IHandlerPipeline pipeline, ILogger logger, IListener listener,
        AdvancedSettings settings)
    {
        Listener = listener;
        _pipeline = pipeline;
        _logger = logger;
        _settings = settings;

        Listener.Start(this, settings.Cancellation);
    }

    public IListener? Listener { get; private set; }

    public void Dispose()
    {
        Listener = null; // making sure the listener can be released
    }

    public async Task ReceivedAsync(Uri uri, Envelope[] messages)
    {
        foreach (var envelope in messages) await ReceivedAsync(uri, envelope);
    }

    public async Task ReceivedAsync(Uri uri, Envelope envelope)
    {
        using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryReceiveSpanName!, envelope,
            ActivityKind.Consumer);

        try
        {
            envelope.MarkReceived(uri, DateTime.UtcNow, _settings.UniqueNodeId);
            await _pipeline.InvokeAsync(envelope, Listener!, activity!);
            _logger.IncomingReceived(envelope, Listener!.Address);

            // TODO -- mark success on the activity?
        }
        catch (Exception? e)
        {
            // TODO -- Mark failures onto the activity?
            _logger.LogError(e, "Failure to receive an incoming message for envelope {EnvelopeId}", envelope.Id);

            try
            {
                await Listener!.DeferAsync(envelope);
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
