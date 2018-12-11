using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Jasper.ApplicationInsights
{
    public class ApplicationInsightsMetrics : IMetrics
    {
        private readonly TelemetryClient _client;
        private readonly JasperOptions _settings;

        public ApplicationInsightsMetrics(TelemetryClient client, JasperOptions settings)
        {
            _client = client;
            _settings = settings;

            _client.Context.Properties.Add("Node", settings.NodeId);
        }

        public void MessageReceived(Envelope envelope)
        {
        }

        public void MessageExecuted(Envelope envelope)
        {
            _client.TrackRequest(new RequestTelemetry
            {
                Duration = TimeSpan.FromMilliseconds(envelope.ExecutionDuration),
                Success = envelope.Succeeded,
                Name = envelope.MessageType,
                Source = envelope.Source,
                Id = envelope.Id.ToString(),
                Timestamp = envelope.SentAt
            });
        }

        public void LogException(Exception ex)
        {
            _client.TrackException(ex);
        }

        public void CircuitBroken(Uri destination)
        {
            _client.TrackEvent(nameof(CircuitBroken),
                new Dictionary<string, string> {{"Destination", destination.ToString()}, {"Node", _settings.NodeId}});
        }

        public void CircuitResumed(Uri destination)
        {
            _client.TrackEvent(nameof(CircuitResumed),
                new Dictionary<string, string> {{"Destination", destination.ToString()}, {"Node", _settings.NodeId}});
        }

        public void LogLocalWorkerQueueDepth(int count)
        {
            _client.TrackMetric(new MetricTelemetry
            {
                Name = "WorkerQueueCount",
                Properties = {{"Node", _settings.NodeId}},
                Count = count
            });
        }

        public void LogPersistedCounts(PersistedCounts counts)
        {
            _client.TrackMetric(new MetricTelemetry
            {
                Name = "PersistedIncomingCount",
                Properties = {{"Node", _settings.NodeId}},
                Count = counts.Incoming
            });

            _client.TrackMetric(new MetricTelemetry
            {
                Name = "PersistedScheduledCount",
                Properties = {{"Node", _settings.NodeId}},
                Count = counts.Scheduled
            });

            _client.TrackMetric(new MetricTelemetry
            {
                Name = "PersistedOutgoingCount",
                Properties = {{"Node", _settings.NodeId}},
                Count = counts.Outgoing
            });
        }

        public void MessagesReceived(IEnumerable<Envelope> envelopes)
        {
            foreach (var group in envelopes.GroupBy(x => x.MessageType))
                _client.TrackMetric(new MetricTelemetry
                {
                    Name = "MessagesReceived",
                    Properties = {{"Node", _settings.NodeId}, {"MessageType", group.Key}},
                    Count = group.Count()
                });
        }
    }
}
