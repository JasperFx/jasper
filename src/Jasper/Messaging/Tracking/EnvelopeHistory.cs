using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Tracking
{
    public class EnvelopeHistory
    {
        private readonly List<EnvelopeRecord> _records = new List<EnvelopeRecord>();

        public EnvelopeHistory(Guid envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public Guid EnvelopeId { get; }

        public Envelope LastEnvelope => _records.LastOrDefault()?.Envelope;

        public object Message
        {
            get
            {
                return _records
                    .FirstOrDefault(x => x.Envelope.Message != null)?.Envelope.Message;
            }
        }

        public IEnumerable<EnvelopeRecord> AllRecords()
        {
            return _records.OrderBy(x => x.SessionTime);
        }

        private EnvelopeRecord lastOf(EventType eventType)
        {
            return _records.LastOrDefault(x => x.EventType == eventType);
        }

        private void markLastCompleted(EventType eventType)
        {
            var record = lastOf(eventType);
            if (record != null)
            {
                record.IsComplete = true;
            }
        }

        /// <summary>
        /// Tracks activity for coordinating the testing of a single Jasper
        /// application
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="envelope"></param>
        /// <param name="sessionTime"></param>
        /// <param name="serviceName"></param>
        /// <param name="exception"></param>
        public void RecordLocally(EventType eventType, Envelope envelope, long sessionTime, string serviceName,
            Exception exception = null)
        {
            var record = new EnvelopeRecord(eventType, envelope, sessionTime, exception) {ServiceName = serviceName};

            switch (eventType)
            {
                case EventType.Sent:
                    // Not tracking anything outgoing
                    // when it's testing locally
                    if (envelope.Destination.Scheme != TransportConstants.Local || envelope.MessageType == TransportConstants.ScheduledEnvelope)
                    {
                        record.IsComplete = true;
                    }

                    if (envelope.Status == TransportConstants.Scheduled)
                    {
                        record.IsComplete = true;
                    }

                    break;

                case EventType.Received:
                    if (envelope.Destination.Scheme == TransportConstants.Local)
                    {
                        markLastCompleted(EventType.Sent);
                    }

                    break;

                case EventType.ExecutionStarted:
                    // Nothing special here
                    break;



                case EventType.ExecutionFinished:
                    markLastCompleted(EventType.ExecutionStarted);
                    record.IsComplete = true;
                    break;

                case EventType.NoHandlers:
                case EventType.NoRoutes:
                case EventType.MessageFailed:
                case EventType.MessageSucceeded:
                    // The message is complete
                    foreach (var envelopeRecord in _records)
                    {
                        envelopeRecord.IsComplete = true;
                    }

                    record.IsComplete = true;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _records.Add(record);
        }

        public void RecordCrossApplication(EventType eventType, Envelope envelope, long sessionTime, string serviceName,
            Exception exception)
        {
            var record = new EnvelopeRecord(eventType, envelope, sessionTime, exception) {ServiceName = serviceName};

            switch (eventType)
            {
                case EventType.Sent:
                    if (envelope.Status == TransportConstants.Scheduled)
                    {
                        record.IsComplete = true;
                    }
                    break;

                case EventType.ExecutionStarted:
                    break;

                case EventType.Received:
                    markLastCompleted(EventType.Sent);
                    break;


                case EventType.ExecutionFinished:
                    markLastCompleted(EventType.ExecutionStarted);
                    record.IsComplete = true;
                    break;

                case EventType.MessageFailed:
                case EventType.MessageSucceeded:
                    // The message is complete
                    foreach (var envelopeRecord in _records)
                    {
                        envelopeRecord.IsComplete = true;
                    }

                    record.IsComplete = true;

                    break;

                case EventType.NoHandlers:
                case EventType.NoRoutes:

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _records.Add(record);
        }

        public bool IsComplete()
        {
            return _records.All(x => x.IsComplete);
        }

        public IEnumerable<EnvelopeRecord> Records => _records;


        public bool Has(EventType eventType)
        {
            return _records.Any(x => x.EventType == eventType);
        }

        public object MessageFor(EventType eventType)
        {
            return _records.Where(x => x.EventType == eventType)
                .LastOrDefault(x => x.Envelope.Message != null)?.Envelope.Message;
        }
    }
}
