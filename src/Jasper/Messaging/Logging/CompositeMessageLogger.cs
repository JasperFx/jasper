using System;
using System.Diagnostics;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;

namespace Jasper.Messaging.Logging
{
    public class CompositeMessageLogger : IMessageLogger
    {
        private readonly IExceptionSink[] _exceptions;

        public CompositeMessageLogger(IMessageEventSink[] sinks, IExceptionSink[] exceptions)
        {
            _exceptions = exceptions;
            Sinks = sinks;
        }

        public IMessageEventSink[] Sinks { get; }

        public void Sent(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.Sent(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Received(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.Received(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void ExecutionStarted(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.ExecutionStarted(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void ExecutionFinished(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.ExecutionFinished(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void MessageSucceeded(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.MessageSucceeded(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.MessageFailed(envelope, ex);
                }
                catch (Exception)
                {
                }
            }
        }

        public void DiscardedEnvelope(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.DiscardedEnvelope(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            try
            {
                ex = ex.Demystify();
            }
            catch (Exception)
            {

            }

            foreach (var sink in _exceptions)
            {
                try
                {
                    sink.LogException(ex, correlationId, message);
                }
                catch (Exception)
                {
                }
            }
        }

        public void NoHandlerFor(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.NoHandlerFor(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void NoRoutesFor(Envelope envelope)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.NoRoutesFor(envelope);
                }
                catch (Exception)
                {
                }
            }
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            foreach (var sink in Sinks)
            {
                try
                {
                    sink.SubscriptionMismatch(mismatch);
                }
                catch (Exception)
                {
                }
            }
        }

        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            try
            {
                ex = ex.Demystify();
            }
            catch (Exception)
            {

            }

            foreach (var sink in Sinks)
            {
                try
                {
                    sink.MovedToErrorQueue(envelope, ex);
                }
                catch (Exception)
                {
                }
            }
        }

        public static CompositeMessageLogger Empty()
        {
            return new CompositeMessageLogger(new IMessageEventSink[0], new IExceptionSink[0]);
        }
    }
}
