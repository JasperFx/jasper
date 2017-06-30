using System;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Lmdb;
using Jasper.Bus.Queues.Logging;
using Jasper.Bus.Queues.Net;
using Jasper.Bus.Queues.Storage;
using Jasper.Bus.Runtime;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Queues.Net
{
    [Collection("SharedTestDirectory")]
    public class SendingErrorPolicyTests : IDisposable
    {
        private readonly ILogger _logger;
        private readonly SendingErrorPolicy _errorPolicy;
        private readonly TestScheduler _scheduler;
        private readonly LmdbMessageStore _store;
        private readonly Subject<OutgoingMessageFailure> _subject;

        public SendingErrorPolicyTests(SharedTestDirectory testDirectory)
        {
            _logger = new RecordingLogger();
            _scheduler = new TestScheduler();
            _store = new LmdbMessageStore(testDirectory.CreateNewDirectoryForTest());
            _subject = new Subject<OutgoingMessageFailure>();
            _errorPolicy = new SendingErrorPolicy(_logger, _store, _subject, _scheduler);
        }

        [Fact]
        public void max_attempts_is_reached()
        {
            var message = ObjectMother.NewMessage<Envelope>();
            message.MaxAttempts = 3;
            message.SentAttempts = 3;
            ShouldBeBooleanExtensions.ShouldBeFalse(_errorPolicy.ShouldRetry(message));
        }

        [Fact]
        public void max_attempts_is_not_reached()
        {
            var message = ObjectMother.NewMessage<Envelope>();
            message.MaxAttempts = 20;
            message.SentAttempts = 5;
            ShouldBeBooleanExtensions.ShouldBeTrue(_errorPolicy.ShouldRetry(message));
        }

        [Fact]
        public void deliver_by_has_expired()
        {
            var message = ObjectMother.NewMessage<Envelope>();
            message.DeliverBy = DateTime.Now.Subtract(TimeSpan.FromSeconds(1));
            message.SentAttempts = 5;
            ShouldBeBooleanExtensions.ShouldBeFalse(_errorPolicy.ShouldRetry(message));
        }

        [Fact]
        public void deliver_by_has_not_expired()
        {
            var message = ObjectMother.NewMessage<Envelope>();
            message.DeliverBy = DateTime.Now.Add(TimeSpan.FromSeconds(1));
            message.SentAttempts = 5;
            ShouldBeBooleanExtensions.ShouldBeTrue(_errorPolicy.ShouldRetry(message));
        }

        [Fact]
        public void has_neither_deliverby_nor_max_attempts()
        {
            var message = ObjectMother.NewMessage<Envelope>();
            message.SentAttempts = 5;
            ShouldBeBooleanExtensions.ShouldBeTrue(_errorPolicy.ShouldRetry(message));
        }

        [Fact]
        public void message_is_observed_after_time()
        {
            Envelope observed = null;
            var message = ObjectMother.NewMessage<Envelope>();
            message.Destination = new Uri("lq.tcp://localhost:5150/blah");
            message.MaxAttempts = 2;
            var tx = _store.BeginTransaction();
            _store.StoreOutgoing(tx, message);
            tx.Commit();
            var failure = new OutgoingMessageFailure();
            failure.Batch = new OutgoingMessageBatch(message.Destination, new []{message});
            using (_errorPolicy.RetryStream.Subscribe(x => { observed = x; }))
            {
                _subject.OnNext(failure);
                _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
                ShouldBeNullExtensions.ShouldNotBeNull(observed);
            }
        }

        [Fact]
        public void message_removed_from_storage_after_max()
        {
            Envelope observed = null;
            var message = ObjectMother.NewMessage<Envelope>();
            message.Destination = new Uri("lq.tcp://localhost:5150/blah");
            message.MaxAttempts = 1;
            var tx = _store.BeginTransaction();
            _store.StoreOutgoing(tx, message);
            tx.Commit();
            var failure = new OutgoingMessageFailure();
            failure.Batch = new OutgoingMessageBatch(message.Destination, new[] {message});
            using (_errorPolicy.RetryStream.Subscribe(x => { observed = x; }))
            {
                _subject.OnNext(failure);
                _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
                ShouldBeNullExtensions.ShouldBeNull(observed);
            }
            ShouldBeBooleanExtensions.ShouldBeFalse(_store.PersistedOutgoingMessages().ToEnumerable().Any());
        }

        [Fact]
        public void time_increases_with_each_failure()
        {
            Envelope observed = null;
            var message = ObjectMother.NewMessage<Envelope>();
            message.Destination = new Uri("lq.tcp://localhost:5150/blah");
            message.MaxAttempts = 5;
            var tx = _store.BeginTransaction();
            _store.StoreOutgoing(tx, message);
            tx.Commit();
            var failure = new OutgoingMessageFailure();
            failure.Batch = new OutgoingMessageBatch(message.Destination, new[] {message});
            using (_errorPolicy.RetryStream.Subscribe(x => { observed = x; }))
            {
                _subject.OnNext(failure);
                _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
                observed.ShouldNotBeNull("first");
                observed = null;
                _subject.OnNext(failure);
                observed.ShouldBeNull("second");
                _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks); //one second isn't enough yet
                observed.ShouldBeNull("third");
                _scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks); //four seconds total for second failure should match
                observed.ShouldNotBeNull("fourth");
            }
        }

        [Fact]
        public void errors_in_storage_dont_end_stream()
        {
            var message = ObjectMother.NewMessage<Envelope>();
            var store = Substitute.For<IMessageStore>();
            store.FailedToSend(Arg.Is(message)).Throws(new Exception("bam!"));
            var errorPolicy = new SendingErrorPolicy(new RecordingLogger(), store, _subject, _scheduler);
            bool ended = false;
            var failure = new OutgoingMessageFailure();
            failure.Batch = new OutgoingMessageBatch(message.Destination, new[] {message});
            using (errorPolicy.RetryStream.Finally(() => ended = true).Subscribe(x => { }))
            {
                _subject.OnNext(failure);
                _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
                ShouldBeBooleanExtensions.ShouldBeFalse(ended);
            }
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }
}
