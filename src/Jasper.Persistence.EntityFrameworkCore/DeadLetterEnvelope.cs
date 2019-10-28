using System;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public class DeadLetterEnvelope
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string MessageType { get; set; }
        public string ExceptionText { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public byte[] Body { get; set; }
        public string Explanation { get; set; }
    }
}
