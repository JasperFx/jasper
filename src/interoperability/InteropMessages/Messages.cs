using System;

namespace InteropMessages
{
    public class InitialMessage
    {
        public Guid Id { get; set; }
    }

    public class ResponseMessage
    {
        public Guid Id { get; set; }
    }

    public class ToJasper
    {
        public Guid Id { get; set; }
    }

    public class ToMassTransit
    {
        public Guid Id { get; set; }
    }
}
