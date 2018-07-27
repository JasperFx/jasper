using System;
using Jasper.Util;

namespace IntegrationTests.Persistence.Marten
{
    [MessageAlias("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageAlias("Message2")]
    public class Message2
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageAlias("Message3")]
    public class Message3
    {

    }

    [MessageAlias("Message4")]
    public class Message4
    {

    }

    [MessageAlias("Message5")]
    public class Message5
    {
        public Guid Id = Guid.NewGuid();

        public int FailThisManyTimes = 0;
    }
}
