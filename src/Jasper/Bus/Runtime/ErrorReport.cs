using System;
using System.Collections.Generic;
using System.IO;
using Baseline;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime
{
    public class ErrorReport
    {
        private static readonly JsonSerializer _serializer;

        static ErrorReport()
        {
            _serializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.None};
        }

        public const string ExceptionDetected = "Exception Detected";

        public ErrorReport(Envelope envelope, Exception ex)
        {
            Headers = envelope.Headers.Clone();
            ExceptionText = ex.ToString();
            ExceptionMessage = ex.Message;
            ExceptionType = ex.GetType().FullName;
            Explanation = ExceptionDetected;
            RawData = envelope.Data;
            Message = envelope.Message;
        }

        public object Message;

        public byte[] RawData { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Explanation { get; set; }

        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionText { get; set; }

        protected bool Equals(ErrorReport other)
        {
            return Equals(Message, other.Message) && string.Equals(ExceptionText, other.ExceptionText);
        }


        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(new StreamWriter(stream), this);
                stream.Position = 0;
                return stream.ReadAllBytes();
            }
        }

        public static ErrorReport Deserialize(byte[] data)
        {
            var stream = new MemoryStream(data) {Position = 0};

            return _serializer.Deserialize<ErrorReport>(new JsonTextReader(new StreamReader(stream)));
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ErrorReport) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Message != null ? Message.GetHashCode() : 0)*397) ^ (ExceptionText != null ? ExceptionText.GetHashCode() : 0);
            }
        }

    }
}