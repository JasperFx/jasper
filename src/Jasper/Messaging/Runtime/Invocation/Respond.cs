using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Invocation
{
    public static class Respond
    {
        public static Response With(object message)
        {
            return new Response(message);
        }
    }

    public class Response : ISendMyself
    {
        private readonly object _message;
        private readonly IList<Action<Envelope, Envelope>> _actions = new List<Action<Envelope, Envelope>>();
        private string _description;



        internal Response(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _description = "Respond '{0}'".ToFormat(message);

            _message = message;
        }

        private Action<Envelope, Envelope> alter
        {
            set
            {
                _actions.Add(value);
            }
        }

        /// <summary>
        /// Set custom headers on the outgoing message envelope
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Response WithHeader(string key, string value)
        {
            alter = (_, e) => e.Headers[key] = value;
            _description += "; {0}='{1}'".ToFormat(key, value);

            return this;
        }

        /// <summary>
        /// Send the response back to the original sender
        /// </summary>
        /// <returns></returns>
        public Response ToSender()
        {
            alter = (old, @new) => @new.Destination = old.ReplyUri;
            _description += "; respond to sender";

            return this;
        }


        /// <summary>
        /// Send the message to a specific destination
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public Response To(string uriString)
        {
            return To(uriString.ToUri());
        }

        /// <summary>
        /// Send the message to a specific destination
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public Response To(Uri destination)
        {
            alter = (_, e) => e.Destination = destination;
            _description += "; Destination=" + destination;

            return this;
        }

        /// <summary>
        /// Scheduled execution until the designated time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Response DelayedUntil(DateTime time)
        {
            alter = (_, e) => e.ExecutionTime = time.ToUniversalTime();
            _description += "; Delayed until " + time.ToUniversalTime();

            return this;
        }

        /// <summary>
        /// Delay the execution by the time designated
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public Response DelayedBy(TimeSpan timeSpan)
        {
            return DelayedUntil(DateTime.UtcNow.Add(timeSpan));
        }

        /// <summary>
        /// Make other customizations to the outgoing Envelope
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        public Response Altered(Action<Envelope> alteration)
        {
            alter = (_, e) => alteration(e);
            return this;
        }

        Envelope ISendMyself.CreateEnvelope(Envelope original)
        {
            var envelope = original.ForResponse(_message);

            _actions.Each(x => x(original, envelope));

            return envelope;
        }

        public override string ToString()
        {
            return _description;
        }
    }
}
