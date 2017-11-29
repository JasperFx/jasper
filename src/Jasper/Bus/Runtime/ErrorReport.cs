using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using Baseline;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime
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
            ContentType = envelope.ContentType;

        }

        public string ContentType { get; set; }

        public string Id { get; set; }

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
