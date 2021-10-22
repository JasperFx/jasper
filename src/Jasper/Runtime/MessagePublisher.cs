using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime.Routing;
using Jasper.Util;
using Lamar;

namespace Jasper.Runtime
{
    public class MessagePublisher : CommandBus, IMessagePublisher
    {
        [DefaultConstructor]
        public MessagePublisher(IMessagingRoot root) : base(root)
        {
        }

        public MessagePublisher(IMessagingRoot root, Guid correlationId) : base(root, correlationId)
        {
        }

        public Task Send<T>(T message)
        {
            var outgoing = Root.Router.RouteOutgoingByMessage(message);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                throw new NoRoutesException(typeof(T));
            }

            return persistOrSend(outgoing);
        }

        public Task PublishEnvelope(Envelope envelope)
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

        public Task Publish<T>(T message)
        {
            var envelope = new Envelope(message);
            return PublishEnvelope(envelope);
        }

        public async Task<Guid> SendEnvelope(Envelope envelope)
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

        public Task SendAndExpectResponseFor<TResponse>(object message, Action<Envelope> customization = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(message);

            customization?.Invoke(envelope);

            return SendEnvelope(envelope);
        }

        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToMessageTypeName();
            Root.Serialization.RegisterType(typeof(TResponse));

            var reader = Root.Serialization.ReaderFor(messageType);

            return new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes
            };
        }

        public Task SendToTopic(object message, string topicName)
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
        public Task SendToDestination<T>(Uri destination, T message)
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
        public Task ScheduleSend<T>(T message, DateTime time)
        {
            // TODO -- optimize this here
            return SendEnvelope(new Envelope
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
        public Task ScheduleSend<T>(T message, TimeSpan delay)
        {
            return ScheduleSend(message, DateTime.UtcNow.Add(delay));
        }

        private void trackEnvelopeCorrelation(Envelope[] outgoing)
        {
            foreach (var outbound in outgoing)
            {
                trackEnvelopeCorrelation(outbound);
            }
        }

        protected virtual void trackEnvelopeCorrelation(Envelope outbound)
        {
            outbound.Source = Root.Settings.ServiceName;
            outbound.CorrelationId = CorrelationId;
        }

        private Task persistOrSend(Envelope envelope)
        {
            if (EnlistedInTransaction)
            {
                _outstanding.Add(envelope);
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

                _outstanding.AddRange(outgoing);
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
