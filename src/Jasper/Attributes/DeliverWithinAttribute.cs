using System;

namespace Jasper.Attributes
{
    // SAMPLE: DeliverWithinAttribute
    /// <summary>
    ///     Directs Jasper that any message of this type must be
    ///     delivered within the supplied number of seconds, or it
    ///     should be discarded
    /// </summary>
    public class DeliverWithinAttribute : ModifyEnvelopeAttribute
    {
        private readonly int _seconds;

        public DeliverWithinAttribute(int seconds)
        {
            _seconds = seconds;
        }

        public override void Modify(Envelope envelope)
        {
            envelope.DeliverBy = DateTimeOffset.UtcNow.AddSeconds(_seconds);
        }
    }

    // ENDSAMPLE
}
