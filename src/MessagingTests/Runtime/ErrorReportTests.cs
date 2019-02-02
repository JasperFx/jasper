using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Shouldly;
using Xunit;

namespace MessagingTests.Runtime
{
    public class ErrorReportTests
    {
        public ErrorReportTests()
        {
            theEnvelope = new Envelope();
            theEnvelope.ContentType = "application/json";
            theEnvelope.Data = new byte[] {1, 2, 3, 4};
            theEnvelope.Source = "OtherApp";
            theEnvelope.Destination = TransportConstants.RepliesUri;

            theException = new TimeoutException("Boo!");

            theErrorReport = new ErrorReport(theEnvelope, theException);
        }

        private readonly Envelope theEnvelope;
        private readonly TimeoutException theException;
        private readonly ErrorReport theErrorReport;

        [Fact]
        public void can_reconstitute_the_envelope()
        {
            var rebuilt = theErrorReport.RebuildEnvelope();
            rebuilt.Id.ShouldBe(theEnvelope.Id);
            rebuilt.Data.ShouldBe(theEnvelope.Data);
        }

        [Fact]
        public void captures_exception_data()
        {
            theErrorReport.ExceptionText.ShouldBe(theException.ToString());
            theErrorReport.ExceptionMessage.ShouldBe(theException.Message);
            theErrorReport.ExceptionType.ShouldBe(theException.GetType().FullName);
        }

        [Fact]
        public void copy_the_id()
        {
            theErrorReport.Id.ShouldBe(theEnvelope.Id);
        }

        [Fact]
        public void copy_the_message_type()
        {
            theErrorReport.MessageType.ShouldBe(theEnvelope.MessageType);
        }

        [Fact]
        public void copy_the_source()
        {
            theErrorReport.Source.ShouldBe(theEnvelope.Source);
        }
    }
}
