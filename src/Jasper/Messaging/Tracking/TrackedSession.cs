using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Lamar;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging.Tracking
{
    public class WaitForMessage<T> : ITrackedCondition
    {
        private readonly TaskCompletionSource<T> _source = new TaskCompletionSource<T>();
        private bool _isCompleted;

        public void Record(Envelope envelope, EventType eventType, string serviceName)
        {
            if (eventType != EventType.MessageSucceeded && eventType != EventType.MessageFailed) return;

            if (envelope.Message is T message)
            {
                if (ServiceName.IsNotEmpty() && ServiceName != serviceName) return;

                _isCompleted = true;
                _source.TrySetResult(message);
            }
        }

        public string ServiceName { get; set; }



        public bool IsCompleted()
        {
            return _isCompleted;
        }
    }

    public interface ITrackedCondition
    {
        void Record(Envelope envelope, EventType eventType, string serviceName);
        bool IsCompleted();
    }

    public class TrackedSession : ITrackedSession
    {
        private readonly Cache<Guid, EnvelopeHistory> _envelopes
            = new Cache<Guid, EnvelopeHistory>(id => new EnvelopeHistory(id));

        private readonly IList<ITrackedCondition> _conditions = new List<ITrackedCondition>();

        private readonly IList<Exception> _exceptions = new List<Exception>();

        private readonly TaskCompletionSource<TrackingStatus> _source;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly IHost _primaryHost;

        private TrackingStatus _status = TrackingStatus.Active;

        private readonly IList<MessageTrackingLogger> _otherHosts = new List<MessageTrackingLogger>();
        private MessageTrackingLogger _primaryLogger;

        public TrackedSession(IHost host)
        {
            _primaryHost = host;
            _source = new TaskCompletionSource<TrackingStatus>();
            _primaryLogger = host?.GetTrackingLogger();
        }

        public void WatchOther(IHost host)
        {
            _otherHosts.Add(host.GetTrackingLogger());

        }

        public TimeSpan Timeout { get; set; } = 5.Seconds();

        public bool AssertNoExceptions { get; set; } = true;

        public Func<IMessageContext, Task> Execution { get; set; } = c => Task.CompletedTask;

        public void AssertNoExceptionsWereThrown()
        {
            if (_exceptions.Any()) throw new AggregateException(_exceptions);
        }

        public void AssertNotTimedOut()
        {
            if (Status == TrackingStatus.TimedOut)
                throw new TimeoutException($"This {nameof(TrackedSession)} timed out before all activity completed.\nActivity detected:\n{AllRecordsInOrder().Select(x => x.ToString()).Join("\n")}");
        }


        public TrackingStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                _source.TrySetResult(value);

                if (value == TrackingStatus.Completed) _stopwatch.Stop();
            }
        }

        public bool AlwaysTrackExternalTransports { get; set; } = false;

        public T FindSingleTrackedMessageOfType<T>()
        {
            return AllRecordsInOrder()
                .Select(x => x.Envelope.Message)
                .OfType<T>()
                .Distinct()
                .Single();
        }

        public IEnumerable<object> UniqueMessages()
        {
            return _envelopes.Select(x => x.Message);
        }

        public IEnumerable<object> UniqueMessages(EventType eventType)
        {
            return _envelopes
                .Where(x => x.Has(eventType))
                .Select(x => x.MessageFor(eventType));
        }

        public T FindSingleTrackedMessageOfType<T>(EventType eventType)
        {
            return AllRecordsInOrder()
                .Where(x => x.EventType == eventType)
                .Select(x => x.Envelope.Message)
                .OfType<T>()
                .Distinct()
                .Single();
        }

        public EnvelopeRecord[] FindEnvelopesWithMessageType<T>(EventType eventType)
        {
            return _envelopes
                .SelectMany(x => x.Records)
                .Where(x => x.EventType == eventType)
                .Where(x => x.Envelope.Message is T)
                .ToArray();

        }

        public EnvelopeRecord[] FindEnvelopesWithMessageType<T>()
        {
            return _envelopes
                .SelectMany(x => x.Records)
                .Where(x => x.Envelope.Message is T)
                .ToArray();
        }

        public EnvelopeRecord[] AllRecordsInOrder()
        {
            return _envelopes.SelectMany(x => x.Records).OrderBy(x => x.SessionTime).ToArray();
        }

        public bool HasNoRecordsOfAnyKind()
        {
            return !_envelopes.Any();
        }

        public EnvelopeRecord[] AllRecordsInOrder(EventType eventType)
        {
            return _envelopes
                .SelectMany(x => x.Records)
                .Where(x => x.EventType == eventType)
                .OrderBy(x => x.SessionTime)
                .ToArray();
        }

        private void setActiveSession(TrackedSession session)
        {
            _primaryLogger.ActiveSession = session;
            foreach (var logger in _otherHosts)
            {
                logger.ActiveSession = session;
            }
        }

        public async Task ExecuteAndTrack()
        {

            setActiveSession(this);

            _stopwatch.Start();




            try
            {
                using (var scope = _primaryHost.Services.As<IContainer>().GetNestedContainer())
                {
                    var context = scope.GetInstance<IMessageContext>();
                    await Execution(context);
                }
            }
            catch (Exception)
            {
                cleanUp();
                throw;
            }

            startTimeoutTracking();

            await _source.Task;

            cleanUp();

            if (AssertNoExceptions) AssertNoExceptionsWereThrown();

            AssertNotTimedOut();
        }

        public Task Track()
        {
            _stopwatch.Start();


            startTimeoutTracking();

            return _source.Task;
        }

        private void cleanUp()
        {
            setActiveSession(null);

            _stopwatch.Stop();
        }

        private void startTimeoutTracking()
        {
#pragma warning disable 4014
            Task.Factory.StartNew(async () =>
#pragma warning restore 4014
            {
                await Task.Delay(Timeout);

                Status = TrackingStatus.TimedOut;
            });
        }

        public void Record(EventType eventType, Envelope envelope, string serviceName, int uniqueNodeId,
            Exception ex = null)
        {
            var history = _envelopes[envelope.Id];

            var record = new EnvelopeRecord(eventType, envelope, _stopwatch.ElapsedMilliseconds, ex)
            {
                ServiceName = serviceName,
                UniqueNodeId = uniqueNodeId
            };

            if (AlwaysTrackExternalTransports || _otherHosts.Any())
            {
                history.RecordCrossApplication(record);
            }
            else
            {
                history.RecordLocally(record);
            }

            if (ex != null) _exceptions.Add(ex);

            if (IsCompleted()) Status = TrackingStatus.Completed;
        }

        public bool IsCompleted()
        {
            if (!_envelopes.All(x => x.IsComplete())) return false;

            return !_conditions.Any() || _conditions.All(x => x.IsCompleted());
        }

        public void LogException(Exception exception, string serviceName)
        {
            _exceptions.Add(exception);
        }
    }
}
