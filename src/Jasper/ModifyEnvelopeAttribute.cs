using System;
using Jasper.Messaging.Runtime;

namespace Jasper
{
    /// <summary>
    /// Base class for an attribute that will customize how
    /// a message type is sent by Jasper by modifying the Envelope
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ModifyEnvelopeAttribute : Attribute
    {
        public abstract void Modify(Envelope envelope);
    }

    /// <summary>
    /// Directs Jasper that any message of this type must be
    /// delivered within the supplied number of seconds, or it
    /// should be discarded
    /// </summary>
    public class DeliverByAttribute : ModifyEnvelopeAttribute
    {
        private readonly int _seconds;

        public DeliverByAttribute(int seconds)
        {
            _seconds = seconds;
        }

        public override void Modify(Envelope envelope)
        {
            envelope.DeliverBy = DateTimeOffset.UtcNow.AddSeconds(_seconds);
        }
    }
}
