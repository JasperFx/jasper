using System;
using Jasper;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Util;

namespace TestMessages
{
    [MessageIdentity("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageIdentity("Message2")]
    public class Message2
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageIdentity("Message3")]
    public class Message3
    {
    }

    [MessageIdentity("Message4")]
    public class Message4
    {
    }

    [MessageIdentity("Message5")]
    public class Message5
    {
        public int FailThisManyTimes = 0;
        public Guid Id = Guid.NewGuid();
    }
}
