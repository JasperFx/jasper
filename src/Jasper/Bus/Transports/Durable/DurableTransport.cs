using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Core;
using Jasper.Util;

namespace Jasper.Bus.Transports.Durable
{
    public class DurableTransport : TransportBase
    {
        public static string ProtocolName = "durable";

        public DurableTransport(CompositeLogger logger, IPersistence persistence)
            : base(ProtocolName, persistence, logger, new SocketSenderProtocal())
        {

        }

        protected override IQueueProvider buildQueueProvider(OutgoingChannels channels)
        {
            return new DurableQueueProvider(Persistence);
        }
    }
}
