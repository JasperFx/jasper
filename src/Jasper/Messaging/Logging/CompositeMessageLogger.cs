using System;
using System.Diagnostics;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;

namespace Jasper.Messaging.Logging
{
    public class CompositeMessageLogger : IMessageLogger
    {
        public CompositeMessageLogger(IMessageLogger[] loggers)
        {
            Loggers = loggers;
        }

        public IMessageLogger[] Loggers { get; }

        public void Sent(Envelope envelope)
        {
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            try
            {
                ex = ex.Demystify();
            }
            catch (Exception)
            {

            }

            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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
            foreach (var sink in Loggers)
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

            foreach (var sink in Loggers)
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
            return new CompositeMessageLogger(new IMessageLogger[0]);
        }
    }
}
