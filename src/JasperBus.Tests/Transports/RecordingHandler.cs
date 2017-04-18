using JasperBus.Runtime;
using JasperBus.Tests.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JasperBus.Tests.Transports
{
    public class RecordingHandler
    {
        private readonly MessageTracker _tracker;

        public RecordingHandler(MessageTracker tracker)
        {
            _tracker = tracker;
        }

        public void Handle(Message1 message, Envelope envelope)
        {
            _tracker.Record(message, envelope);
        }
    }
}
