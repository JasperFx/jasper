using JasperBus.Configuration;
using JasperBus.Runtime;

namespace JasperBus.Serializers
{
    public interface IEnvelopeSerializer
    {
        object Deserialize(Envelope envelope, ChannelNode node);
        void Serialize(Envelope envelope, ChannelNode node);
    }
}