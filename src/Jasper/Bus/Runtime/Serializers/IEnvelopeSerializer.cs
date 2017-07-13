using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Jasper.Bus.Configuration;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Serializers
{
    public interface IEnvelopeSerializer
    {
        object Deserialize(Envelope envelope, ChannelNode node);
        void Serialize(Envelope envelope, ChannelNode node);
    }
}
