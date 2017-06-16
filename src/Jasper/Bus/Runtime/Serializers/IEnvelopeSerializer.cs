using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Serializers
{
    public interface IEnvelopeSerializer
    {
        object Deserialize(Envelope envelope, ChannelNode node);
        void Serialize(Envelope envelope, ChannelNode node);
    }
}