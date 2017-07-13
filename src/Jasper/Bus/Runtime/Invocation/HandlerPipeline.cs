using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        Task Invoke(Envelope envelope, ChannelNode receiver);
        IBusLogger Logger { get; }
        Task InvokeNow(object message);
        Task InvokeNow(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSender _sender;
        private readonly SerializationGraph _serializer;
        private readonly HandlerGraph _graph;
        private readonly IReplyWatcher _replies;
        private readonly IDelayedJobProcessor _delayedJobs;
        private readonly IInMemoryQueue _inMemoryQueue;
        private readonly IMissingHandler[] _missingHandlers;

        public HandlerPipeline(IEnvelopeSender sender, SerializationGraph serializers, HandlerGraph graph, IReplyWatcher replies, IDelayedJobProcessor delayedJobs, IInMemoryQueue inMemoryQueue, IBusLogger[] loggers, IMissingHandler[] missingHandlers)
        {
            _sender = sender;
            _serializer = serializers;
            _graph = graph;
            _replies = replies;
            _delayedJobs = delayedJobs;
            _inMemoryQueue = inMemoryQueue;
            _missingHandlers = missingHandlers;

            Logger = BusLogger.Combine(loggers);
        }

        public IBusLogger Logger { get; }

        public async Task Invoke(Envelope envelope, ChannelNode receiver)
        {
            var now = DateTime.UtcNow;

            try
            {
                await invoke(envelope, receiver, now).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Gotta get the message out of here because it's something that
                // could never be handled
                await envelope.Callback.MoveToErrors(new ErrorReport(envelope, e));
                Logger.LogException(e, envelope.CorrelationId);
            }
        }

        private async Task invoke(Envelope envelope, ChannelNode receiver, DateTime now)
        {
            using (var context = new EnvelopeContext(this, envelope, _sender, _delayedJobs))
            {
                if (envelope.IsDelayed(now))
                {
                    await moveToDelayedMessageQueue(envelope, context);
                }
                else if (envelope.ResponseId.IsNotEmpty())
                {
                    completeRequestWithRequestedResponse(envelope, receiver);
                }
                else
                {
                    try
                    {
                        deserialize(envelope, receiver);
                    }
                    catch (Exception e)
                    {
                        Logger.MessageFailed(envelope, e);
                        await envelope.Callback.MoveToErrors(new ErrorReport(envelope, e));
                        return;
                    }
                    finally
                    {
                        Logger.Received(envelope);
                    }

                    await ProcessMessage(envelope, context).ConfigureAwait(false);
                }
            }
        }

        public Task InvokeNow(object message)
        {
            var envelope = _inMemoryQueue.EnvelopeForInlineMessage(message);

            return InvokeNow(envelope);
        }

        public Task InvokeNow(Envelope envelope)
        {
            using (var context = new EnvelopeContext(this, envelope, _sender, _delayedJobs))
            {
                return ProcessMessage(envelope, context);
            }
        }


        private void deserialize(Envelope envelope, ChannelNode receiver)
        {
            // TODO -- Not super duper wild about this one.
            if (envelope.Message == null)
            {
                envelope.Message = _serializer.Deserialize(envelope, receiver);
            }
        }

        public async Task ProcessMessage(Envelope envelope, EnvelopeContext context)
        {
            Logger.ExecutionStarted(envelope);

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                processNoHandlerLogic(envelope);
            }
            else
            {
                // TODO -- have the EnvelopeContext.Retry be able to skip right down
                // to the executeChain method here
                var continuation = await executeChain(handler, context).ConfigureAwait(false);

                await continuation.Execute(envelope, context, DateTime.UtcNow).ConfigureAwait(false);
            }
        }

        private async Task<IContinuation> executeChain(MessageHandler handler, EnvelopeContext context)
        {
            try
            {
                context.Envelope.Attempts++;

                await handler.Handle(context).ConfigureAwait(false);

                Logger.ExecutionFinished(context.Envelope);

                return MessageSucceededContinuation.Instance;
            }
            catch (Exception e)
            {
                Logger.LogException(e, context.Envelope.CorrelationId, "Failure during message processing execution");
                return context.DetermineContinuation(e, handler.Chain, _graph);
            }
        }

        private void processNoHandlerLogic(Envelope envelope)
        {
            Logger.NoHandlerFor(envelope);

            foreach (var handler in _missingHandlers)
            {
                try
                {
                    handler.Handle(envelope);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
        }

        private void completeRequestWithRequestedResponse(Envelope envelope, ChannelNode receiver)
        {
            try
            {
                deserialize(envelope, receiver);
                _replies.Handle(envelope);
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.CorrelationId, "Failure during reply handling.");
            }
        }

        private async Task moveToDelayedMessageQueue(Envelope envelope, EnvelopeContext context)
        {
            try
            {
                await envelope.Callback.MoveToDelayedUntil(envelope, _delayedJobs, envelope.ExecutionTime.Value.ToUniversalTime());
            }
            catch (Exception e)
            {
                await envelope.Callback.MarkFailed(e);
                context.Logger.LogException(e, envelope.CorrelationId, "Failed to move delayed message to the delayed message queue");
            }
        }
    }

    [Obsolete("Replace this usage with an in-memory callback within the Consume()")]
    public class InlineMessageCallback : IMessageCallback
    {
        private readonly object _message;

        public InlineMessageCallback(object message)
        {
            _message = message;
        }

        public Task MarkSuccessful()
        {
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            throw new InlineMessageException("Failed while invoking an inline message", ex);
        }

        public Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time)
        {
            throw new NotImplementedException();
        }

        public Task MoveToErrors(ErrorReport report)
        {
            // TODO -- need a general way to log errors against an ITransport
            return Task.CompletedTask;
        }

        public Task Send(Envelope envelope)
        {
            // nothing
            return Task.CompletedTask;
        }

        public bool SupportsSend { get; } = false;
        public Task Requeue(Envelope envelope)
        {
            throw new NotImplementedException();
        }
    }


    public class InlineMessageException : Exception
    {
        public InlineMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
