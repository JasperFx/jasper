using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime.Routing;
using Jasper.Util;
using Lamar;

namespace Jasper.Runtime
{
    public class MessagePublisher : CommandBus, IMessagePublisher
    {
        [DefaultConstructor]
        public MessagePublisher(IJasperRuntime root) : base(root)
        {
        }

        public MessagePublisher(IJasperRuntime root, string? correlationId) : base(root, correlationId)
        {
        }

        public Task SendAsync<T>(T? message)
        {
            var outgoing = Root.Router.RouteOutgoingByMessage(message);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                throw new NoRoutesException(typeof(T));
            }

            return persistOrSend(outgoing);
        }

        public Task PublishEnvelopeAsync(Envelope? envelope)
        {
            if (envelope.Message == null && envelope.Data == null)
                throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = Root.Router.RouteOutgoingByEnvelope(envelope);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                Root.MessageLogger.NoRoutesFor(envelope);
                return Task.CompletedTask;
            }

            return persistOrSend(outgoing);
        }

        public Task PublishAsync<T>(T? message)
        {
            var envelope = new Envelope(message);
            return PublishEnvelopeAsync(envelope);
        }

        public async Task<Guid> SendEnvelopeAsync(Envelope? envelope)
        {
            if (envelope.Message == null && envelope.Data == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = Root.Router.RouteOutgoingByEnvelope(envelope);

            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                Root.MessageLogger.NoRoutesFor(envelope);

                throw new NoRoutesException(envelope);
            }

            await persistOrSend(outgoing);

            return envelope.Id;
        }

        public Task SendAndExpectResponseForAsync<TResponse>(object message, Action<Envelope?>? customization = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(message);

            customization?.Invoke(envelope);

            return SendEnvelopeAsync(envelope);
        }

        public Envelope? EnvelopeForRequestResponse<TResponse>(object request)
        {
            return new Envelope
            {
                Message = request,
                ReplyRequested = typeof(TResponse).ToMessageTypeName(), // memoize this maybe?
                AcceptedContentTypes = new []{EnvelopeConstants.JsonContentType} // TODO -- might want a default serializer option for here
            };
        }

        public Task SendToTopicAsync(object? message, string? topicName)
        {
            var envelope = new Envelope(message)
            {
                TopicName = topicName
            };

            var outgoing = Root.Router.RouteToTopic(topicName, envelope);
            return persistOrSend(outgoing);
        }

        /// <summary>
        ///     Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        public Task SendToDestinationAsync<T>(Uri? destination, T? message)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var envelope = new Envelope {Message = message, Destination = destination};
            Root.Router.RouteToDestination(destination, envelope);

            trackEnvelopeCorrelation(envelope);

            return persistOrSend(envelope);
        }

        /// <summary>
        ///     Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        public Task ScheduleSendAsync<T>(T message, DateTime time)
        {
            return SendEnvelopeAsync(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime(),
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

        private void trackEnvelopeCorrelation(Envelope?[] outgoing)
        {
            foreach (var outbound in outgoing)
            {
                trackEnvelopeCorrelation(outbound);
            }
        }

        protected virtual void trackEnvelopeCorrelation(Envelope? outbound)
        {
            outbound.Source = Root.Settings.ServiceName;
            outbound.CorrelationId = CorrelationId;
        }

        private Task persistOrSend(Envelope envelope)
        {
            if (EnlistedInTransaction)
            {
                _outstanding.Fill(envelope);
                return envelope.Sender.IsDurable ? Transaction.Persist(envelope) : Task.CompletedTask;
            }
            else
            {
                return envelope.Send();
            }
        }

        private async Task persistOrSend(params Envelope[] outgoing)
        {
            if (EnlistedInTransaction)
            {
                await Transaction.Persist(outgoing.Where(isDurable).ToArray());

                _outstanding.Fill(outgoing);
            }
            else
            {
                foreach (var outgoingEnvelope in outgoing)
                {
                    await outgoingEnvelope.Send();
                }
            }
        }

        private bool isDurable(Envelope envelope)
        {
            if (envelope.Sender != null) return envelope.Sender.IsDurable;

            if (envelope.Destination != null) return Root.Runtime.GetOrBuildSendingAgent(envelope.Destination).IsDurable;

            return false;
        }
    }
}
