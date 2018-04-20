using System;

namespace Jasper.Messaging.Runtime
{
    public class ErrorReport
    {
        public const string ExceptionDetected = "Exception Detected";

        public ErrorReport()
        {
        }

        public ErrorReport(Envelope envelope, Exception ex)
        {
            ExceptionText = ex.ToString();
            ExceptionMessage = ex.Message;
            ExceptionType = ex.GetType().FullName;
            Explanation = ExceptionDetected;

            try
            {
                RawData = envelope.Serialize();
            }
            catch (Exception)
            {
                // Nothing
            }

            MessageType = envelope.MessageType;
            Source = envelope.Source;
            Id = envelope.Id;
        }


        public Guid Id { get; set; }

        public string Source { get; set; }
        public string MessageType { get; set; }


        public byte[] RawData { get; set; }


        public string Explanation { get; set; }

        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionText { get; set; }


        public Envelope RebuildEnvelope()
        {
            return Envelope.Read(RawData);
        }
    }
}
