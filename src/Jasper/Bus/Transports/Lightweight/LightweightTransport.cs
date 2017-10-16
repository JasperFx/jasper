using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Durable;
using Jasper.Bus.Transports.Loopback;
using Jasper.Util;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightTransport : TransportBase
    {
        public static readonly string ProtocolName = "tcp";

        public LightweightTransport(CompositeLogger logger, BusSettings settings)
            : base(ProtocolName, new NulloPersistence(), logger, new SocketSenderProtocol(), settings)
        {


        }

        protected override IQueueProvider buildQueueProvider(OutgoingChannels channels)
        {
            return new LightweightQueueProvider(() => channels[TransportConstants.RetryUri]);
        }
    }
}
